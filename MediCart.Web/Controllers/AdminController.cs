using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MediCart.Web.Data;
using MediCart.Web.Models;
// using Microsoft.AspNetCore.Authorization; // enable once every teammate has tested login with the Admin role

namespace MediCart.Web.Controllers
{
    // [Authorize(Roles = "Admin")] // TODO: turn this on before demo — currently anyone can hit /Admin/*
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AdminController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Profile()
        {
            return View();
        }

        // TEMP placeholder stubs so sidebar links don't 404 — replace each
        // with a real action + view as those pages get built.
        [HttpGet] public IActionResult Dashboard() => View("ComingSoon");
        [HttpGet] public IActionResult Medicines() => View("ComingSoon");
        [HttpGet] public IActionResult IncomingOrders() => View("ComingSoon");
        [HttpGet] public IActionResult FlaggedOrders() => View("ComingSoon");
        [HttpGet] public IActionResult StockExpiry() => View("ComingSoon");
        [HttpGet] public IActionResult AuditLog() => View("ComingSoon");
        [HttpGet] public IActionResult ContactMessages() => View("ComingSoon");

        // ===================== Categories & Product Types =====================

        [HttpGet]
        public async Task<IActionResult> Categories()
        {
            var model = await BuildCategoriesViewModelAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(CategoryFormViewModel form)
        {
            if (!ModelState.IsValid)
            {
                TempData["CategoryError"] = "Please enter a valid category name.";
                return RedirectToAction(nameof(Categories));
            }

            var duplicate = await _db.Categories.AnyAsync(c =>
                c.Name.ToLower() == form.Name.Trim().ToLower() && c.ParentCategoryId == form.ParentCategoryId);

            if (duplicate)
            {
                TempData["CategoryError"] = $"'{form.Name}' already exists at this level.";
                return RedirectToAction(nameof(Categories));
            }

            var category = new Category
            {
                Name = form.Name.Trim(),
                Description = string.IsNullOrWhiteSpace(form.Description) ? null : form.Description.Trim(),
                ParentCategoryId = form.ParentCategoryId,
                CreatedAt = DateTime.UtcNow
            };

            _db.Categories.Add(category);
            await _db.SaveChangesAsync();

            TempData["CategorySuccess"] = $"Category '{category.Name}' added.";
            return RedirectToAction(nameof(Categories));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(CategoryFormViewModel form)
        {
            if (form.Id is null || !ModelState.IsValid)
            {
                TempData["CategoryError"] = "Please enter a valid category name.";
                return RedirectToAction(nameof(Categories));
            }

            var category = await _db.Categories.FindAsync(form.Id.Value);
            if (category == null)
            {
                TempData["CategoryError"] = "Category not found.";
                return RedirectToAction(nameof(Categories));
            }

            if (form.ParentCategoryId == category.Id)
            {
                TempData["CategoryError"] = "A category cannot be its own parent.";
                return RedirectToAction(nameof(Categories));
            }

            if (form.ParentCategoryId.HasValue && await IsDescendantAsync(category.Id, form.ParentCategoryId.Value))
            {
                TempData["CategoryError"] = "Cannot move a category under its own subcategory.";
                return RedirectToAction(nameof(Categories));
            }

            category.Name = form.Name.Trim();
            category.Description = string.IsNullOrWhiteSpace(form.Description) ? null : form.Description.Trim();
            category.ParentCategoryId = form.ParentCategoryId;

            await _db.SaveChangesAsync();

            TempData["CategorySuccess"] = $"Category '{category.Name}' updated.";
            return RedirectToAction(nameof(Categories));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _db.Categories
                .Include(c => c.SubCategories)
                .Include(c => c.Medicines)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                TempData["CategoryError"] = "Category not found.";
                return RedirectToAction(nameof(Categories));
            }

            if (category.SubCategories.Count > 0)
            {
                TempData["CategoryError"] =
                    $"Cannot delete '{category.Name}' — it has {category.SubCategories.Count} subcategor{(category.SubCategories.Count == 1 ? "y" : "ies")}. Delete or reassign those first.";
                return RedirectToAction(nameof(Categories));
            }

            if (category.Medicines.Count > 0)
            {
                TempData["CategoryError"] =
                    $"Cannot delete '{category.Name}' — {category.Medicines.Count} medicine(s) still use it. Reassign them first.";
                return RedirectToAction(nameof(Categories));
            }

            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();

            TempData["CategorySuccess"] = $"Category '{category.Name}' deleted.";
            return RedirectToAction(nameof(Categories));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProductType(ProductTypeFormViewModel form)
        {
            if (!ModelState.IsValid)
            {
                TempData["ProductTypeError"] = "Please enter a valid product type name.";
                return RedirectToAction(nameof(Categories));
            }

            var duplicate = await _db.ProductTypes.AnyAsync(p => p.Name.ToLower() == form.Name.Trim().ToLower());
            if (duplicate)
            {
                TempData["ProductTypeError"] = $"'{form.Name}' already exists.";
                return RedirectToAction(nameof(Categories));
            }

            _db.ProductTypes.Add(new ProductType { Name = form.Name.Trim() });
            await _db.SaveChangesAsync();

            TempData["ProductTypeSuccess"] = $"Product type '{form.Name}' added.";
            return RedirectToAction(nameof(Categories));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProductType(ProductTypeFormViewModel form)
        {
            if (form.Id is null || !ModelState.IsValid)
            {
                TempData["ProductTypeError"] = "Please enter a valid product type name.";
                return RedirectToAction(nameof(Categories));
            }

            var productType = await _db.ProductTypes.FindAsync(form.Id.Value);
            if (productType == null)
            {
                TempData["ProductTypeError"] = "Product type not found.";
                return RedirectToAction(nameof(Categories));
            }

            var duplicate = await _db.ProductTypes.AnyAsync(p =>
                p.Id != productType.Id && p.Name.ToLower() == form.Name.Trim().ToLower());

            if (duplicate)
            {
                TempData["ProductTypeError"] = $"'{form.Name}' already exists.";
                return RedirectToAction(nameof(Categories));
            }

            productType.Name = form.Name.Trim();
            await _db.SaveChangesAsync();

            TempData["ProductTypeSuccess"] = "Product type updated.";
            return RedirectToAction(nameof(Categories));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProductType(int id)
        {
            var productType = await _db.ProductTypes
                .Include(p => p.Medicines)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (productType == null)
            {
                TempData["ProductTypeError"] = "Product type not found.";
                return RedirectToAction(nameof(Categories));
            }

            if (productType.Medicines.Count > 0)
            {
                TempData["ProductTypeError"] =
                    $"Cannot delete '{productType.Name}' — {productType.Medicines.Count} medicine(s) still use it.";
                return RedirectToAction(nameof(Categories));
            }

            _db.ProductTypes.Remove(productType);
            await _db.SaveChangesAsync();

            TempData["ProductTypeSuccess"] = $"Product type '{productType.Name}' deleted.";
            return RedirectToAction(nameof(Categories));
        }

        // ===================== Helpers =====================

        private async Task<AdminCategoriesViewModel> BuildCategoriesViewModelAsync()
        {
            var categories = await _db.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.SubCategories)
                .Include(c => c.Medicines)
                .OrderBy(c => c.ParentCategoryId == null ? 0 : 1)
                .ThenBy(c => c.Name)
                .ToListAsync();

            var productTypes = await _db.ProductTypes
                .Include(p => p.Medicines)
                .OrderBy(p => p.Name)
                .ToListAsync();

            return new AdminCategoriesViewModel
            {
                Categories = categories.Select(c => new CategoryRowViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ParentCategoryId = c.ParentCategoryId,
                    ParentCategoryName = c.ParentCategory?.Name,
                    SubCategoryCount = c.SubCategories.Count,
                    MedicineCount = c.Medicines.Count
                }).ToList(),

                ProductTypes = productTypes.Select(p => new ProductTypeRowViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    MedicineCount = p.Medicines.Count
                }).ToList(),

                ParentCategoryOptions = categories.Select(c => new CategoryOptionViewModel
                {
                    Id = c.Id,
                    Name = c.Name
                }).ToList()
            };
        }

        // Walks up the parent chain from candidateParentId to check whether
        // categoryId appears as an ancestor — prevents circular category trees.
        private async Task<bool> IsDescendantAsync(int categoryId, int candidateParentId)
        {
            int? currentId = candidateParentId;

            while (currentId.HasValue)
            {
                if (currentId.Value == categoryId) return true;

                currentId = await _db.Categories
                    .Where(c => c.Id == currentId.Value)
                    .Select(c => c.ParentCategoryId)
                    .FirstOrDefaultAsync();
            }

            return false;
        }
    }
}
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
        private readonly Services.IImageUploadService _imageUploadService;

        public AdminController(
            ApplicationDbContext db,
            Services.IImageUploadService imageUploadService)
        {
            _db = db;
            _imageUploadService = imageUploadService;
        }

        [HttpGet]
        public IActionResult Profile()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Medicines(
            string? search,
            int? categoryId,
            int? productTypeId)
        {
            var query = _db.Medicines
                .Include(m => m.Category)
                .Include(m => m.ProductType)
                .Include(m => m.Stock)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(m =>
                    EF.Functions.ILike(m.Name, $"%{term}%"));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(m =>
                    m.CategoryId == categoryId.Value);
            }

            if (productTypeId.HasValue)
            {
                query = query.Where(m =>
                    m.ProductTypeId == productTypeId.Value);
            }

            var medicines = await query
                .OrderBy(m => m.Name)
                .Select(m => new MedicineListRowViewModel
                {
                    Id = m.Id,
                    Name = m.Name,
                    CategoryName = m.Category.Name,
                    ProductTypeName = m.ProductType.Name,
                    Manufacturer = m.Manufacturer,
                    Price = m.Price,
                    StockQuantity = m.Stock != null
                        ? m.Stock.Quantity
                        : 0,
                    ExpiryDate = m.Stock != null
                        ? m.Stock.ExpiryDate
                        : DateOnly.MinValue,
                    SensitivityLevel = m.SensitivityLevel,
                    RequiresPrescription = m.RequiresPrescription
                })
                .ToListAsync();

            var model = new AdminMedicinesPageViewModel
            {
                Medicines = medicines,
                CategoryOptions =
                    await BuildCategoryDropdownOptionsAsync(),
                ProductTypeOptions =
                    await BuildProductTypeDropdownOptionsAsync(),
                Search = search,
                CategoryId = categoryId,
                ProductTypeId = productTypeId,
                TotalCount = medicines.Count
            };

            return View(model);
        }

        // TEMP placeholder stubs so sidebar links don't 404
        [HttpGet]
        public IActionResult Dashboard() => View("ComingSoon");

        [HttpGet]
        public IActionResult IncomingOrders() => View("ComingSoon");

        [HttpGet]
        public IActionResult FlaggedOrders() => View("ComingSoon");

        [HttpGet]
        public IActionResult StockExpiry() => View("ComingSoon");

        [HttpGet]
        public IActionResult AuditLog() => View("ComingSoon");

        [HttpGet]
        public IActionResult ContactMessages() => View("ComingSoon");


        // =====================
        // Categories & Product Types
        // =====================

        [HttpGet]
        public async Task<IActionResult> Categories()
        {
            var model = await BuildCategoriesViewModelAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(
            CategoryFormViewModel form)
        {
            if (!ModelState.IsValid)
            {
                TempData["CategoryError"] =
                    "Please enter a valid category name.";

                return RedirectToAction(nameof(Categories));
            }

            var duplicate = await _db.Categories.AnyAsync(c =>
                c.Name.ToLower() ==
                form.Name.Trim().ToLower()
                &&
                c.ParentCategoryId == form.ParentCategoryId);

            if (duplicate)
            {
                TempData["CategoryError"] =
                    $"'{form.Name}' already exists at this level.";

                return RedirectToAction(nameof(Categories));
            }

            var category = new Category
            {
                Name = form.Name.Trim(),
                Description =
                    string.IsNullOrWhiteSpace(form.Description)
                        ? null
                        : form.Description.Trim(),
                ParentCategoryId = form.ParentCategoryId,
                CreatedAt = DateTime.UtcNow
            };

            _db.Categories.Add(category);
            await _db.SaveChangesAsync();

            TempData["CategorySuccess"] =
                $"Category '{category.Name}' added.";

            return RedirectToAction(nameof(Categories));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(
            CategoryFormViewModel form)
        {
            if (form.Id is null || !ModelState.IsValid)
            {
                TempData["CategoryError"] =
                    "Please enter a valid category name.";

                return RedirectToAction(nameof(Categories));
            }

            var category =
                await _db.Categories.FindAsync(form.Id.Value);

            if (category == null)
            {
                TempData["CategoryError"] =
                    "Category not found.";

                return RedirectToAction(nameof(Categories));
            }

            if (form.ParentCategoryId == category.Id)
            {
                TempData["CategoryError"] =
                    "A category cannot be its own parent.";

                return RedirectToAction(nameof(Categories));
            }

            if (form.ParentCategoryId.HasValue &&
                await IsDescendantAsync(
                    category.Id,
                    form.ParentCategoryId.Value))
            {
                TempData["CategoryError"] =
                    "Cannot move a category under its own subcategory.";

                return RedirectToAction(nameof(Categories));
            }

            category.Name = form.Name.Trim();

            category.Description =
                string.IsNullOrWhiteSpace(form.Description)
                    ? null
                    : form.Description.Trim();

            category.ParentCategoryId =
                form.ParentCategoryId;

            await _db.SaveChangesAsync();

            TempData["CategorySuccess"] =
                $"Category '{category.Name}' updated.";

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
                TempData["CategoryError"] =
                    "Category not found.";

                return RedirectToAction(nameof(Categories));
            }

            if (category.SubCategories.Count > 0)
            {
                TempData["CategoryError"] =
                    $"Cannot delete '{category.Name}' — it has " +
                    $"{category.SubCategories.Count} " +
                    $"subcategor{(category.SubCategories.Count == 1 ? "y" : "ies")}. " +
                    "Delete or reassign those first.";

                return RedirectToAction(nameof(Categories));
            }

            if (category.Medicines.Count > 0)
            {
                TempData["CategoryError"] =
                    $"Cannot delete '{category.Name}' — " +
                    $"{category.Medicines.Count} medicine(s) still use it. " +
                    "Reassign them first.";

                return RedirectToAction(nameof(Categories));
            }

            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();

            TempData["CategorySuccess"] =
                $"Category '{category.Name}' deleted.";

            return RedirectToAction(nameof(Categories));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProductType(
            ProductTypeFormViewModel form)
        {
            if (!ModelState.IsValid)
            {
                TempData["ProductTypeError"] =
                    "Please enter a valid product type name.";

                return RedirectToAction(nameof(Categories));
            }

            var duplicate =
                await _db.ProductTypes.AnyAsync(p =>
                    p.Name.ToLower() ==
                    form.Name.Trim().ToLower());

            if (duplicate)
            {
                TempData["ProductTypeError"] =
                    $"'{form.Name}' already exists.";

                return RedirectToAction(nameof(Categories));
            }

            _db.ProductTypes.Add(new ProductType
            {
                Name = form.Name.Trim()
            });

            await _db.SaveChangesAsync();

            TempData["ProductTypeSuccess"] =
                $"Product type '{form.Name}' added.";

            return RedirectToAction(nameof(Categories));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProductType(
            ProductTypeFormViewModel form)
        {
            if (form.Id is null || !ModelState.IsValid)
            {
                TempData["ProductTypeError"] =
                    "Please enter a valid product type name.";

                return RedirectToAction(nameof(Categories));
            }

            var productType =
                await _db.ProductTypes.FindAsync(form.Id.Value);

            if (productType == null)
            {
                TempData["ProductTypeError"] =
                    "Product type not found.";

                return RedirectToAction(nameof(Categories));
            }

            var duplicate =
                await _db.ProductTypes.AnyAsync(p =>
                    p.Id != productType.Id &&
                    p.Name.ToLower() ==
                    form.Name.Trim().ToLower());

            if (duplicate)
            {
                TempData["ProductTypeError"] =
                    $"'{form.Name}' already exists.";

                return RedirectToAction(nameof(Categories));
            }

            productType.Name = form.Name.Trim();

            await _db.SaveChangesAsync();

            TempData["ProductTypeSuccess"] =
                "Product type updated.";

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
                TempData["ProductTypeError"] =
                    "Product type not found.";

                return RedirectToAction(nameof(Categories));
            }

            if (productType.Medicines.Count > 0)
            {
                TempData["ProductTypeError"] =
                    $"Cannot delete '{productType.Name}' — " +
                    $"{productType.Medicines.Count} medicine(s) still use it.";

                return RedirectToAction(nameof(Categories));
            }

            _db.ProductTypes.Remove(productType);
            await _db.SaveChangesAsync();

            TempData["ProductTypeSuccess"] =
                $"Product type '{productType.Name}' deleted.";

            return RedirectToAction(nameof(Categories));
        }


        // =====================
        // Add Medicine
        // =====================

        [HttpGet]
        public async Task<IActionResult> AddMedicine()
        {
            var model = new AdminMedicineFormPageViewModel
            {
                Form = new MedicineFormViewModel
                {
                    ExpiryDate =
                        DateOnly.FromDateTime(
                            DateTime.UtcNow.AddYears(2))
                },
                CategoryOptions =
                    await BuildCategoryDropdownOptionsAsync(),
                ProductTypeOptions =
                    await BuildProductTypeDropdownOptionsAsync()
            };

            return View(model);
        }

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> AddMedicine(
    AdminMedicineFormPageViewModel model)
{
    var form = model.Form;

    form.SideEffects = form.SideEffects
        .Where(se =>
            !string.IsNullOrWhiteSpace(se.Effect))
        .ToList();

    string? finalImageUrl = form.ImageUrl;

    // Handle medicine image upload
    if (form.ImageFile != null &&
        form.ImageFile.Length > 0)
    {
        if (!IsValidImageFile(form.ImageFile))
        {
            ModelState.AddModelError(
                "Form.ImageFile",
                "Upload a JPG, PNG, or WEBP image under 5MB.");
        }
        else
        {
            try
            {
                finalImageUrl =
                    await _imageUploadService
                        .UploadMedicineImageAsync(
                            form.ImageFile);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(
                    "Form.ImageFile",
                    ex.Message);
            }
        }
    }

    // If validation fails, return to the form
    if (!ModelState.IsValid)
    {
        model.CategoryOptions =
            await BuildCategoryDropdownOptionsAsync();

        model.ProductTypeOptions =
            await BuildProductTypeDropdownOptionsAsync();

        return View(model);
    }

    // Check that the selected category exists
    var categoryExists =
        await _db.Categories.AnyAsync(c =>
            c.Id == form.CategoryId);

    // Check that the selected product type exists
    var productTypeExists =
        await _db.ProductTypes.AnyAsync(p =>
            p.Id == form.ProductTypeId);

    if (!categoryExists ||
        !productTypeExists)
    {
        ModelState.AddModelError(
            string.Empty,
            "The selected category or product type " +
            "no longer exists — refresh the page.");

        model.CategoryOptions =
            await BuildCategoryDropdownOptionsAsync();

        model.ProductTypeOptions =
            await BuildProductTypeDropdownOptionsAsync();

        return View(model);
    }

    // Create the medicine
    var medicine = new Medicine
    {
        Name = form.Name.Trim(),

        CategoryId = form.CategoryId,

        ProductTypeId = form.ProductTypeId,

        Manufacturer =
            string.IsNullOrWhiteSpace(form.Manufacturer)
                ? null
                : form.Manufacturer.Trim(),

        GenericName =
            string.IsNullOrWhiteSpace(form.GenericName)
                ? null
                : form.GenericName.Trim(),

        Unit =
            string.IsNullOrWhiteSpace(form.Unit)
                ? null
                : form.Unit.Trim(),

        Description =
            string.IsNullOrWhiteSpace(form.Description)
                ? null
                : form.Description.Trim(),

        ImageUrl =
            string.IsNullOrWhiteSpace(finalImageUrl)
                ? null
                : finalImageUrl.Trim(),

        Price = form.Price,

        RequiresPrescription =
            form.RequiresPrescription,

        SensitivityLevel =
            string.IsNullOrWhiteSpace(form.SensitivityLevel)
                ? null
                : form.SensitivityLevel,

        CreatedAt = DateTime.UtcNow,

        // Create stock together with the medicine
        Stock = new Stock
        {
            Quantity = form.StockQuantity,

            ExpiryDate = form.ExpiryDate,

            UpdatedAt = DateTime.UtcNow
        },

        // Create side effects together with the medicine
        SideEffects = form.SideEffects
            .Select(se => new SideEffect
            {
                Effect = se.Effect.Trim(),

                Severity = se.Severity
            })
            .ToList()
    };

    // Add medicine to EF Core
    _db.Medicines.Add(medicine);

    // Save medicine + stock + side effects to PostgreSQL
    await _db.SaveChangesAsync();

    // Show success message after redirect
    TempData["MedicineSuccess"] =
        $"Medicine '{medicine.Name}' added.";

    // Redirect to the medicine list
    return RedirectToAction(nameof(Medicines));
}


        // =====================
        // Edit Medicine
        // =====================

        [HttpGet]
        public async Task<IActionResult> EditMedicine(int id)
        {
            var medicine = await _db.Medicines
                .Include(m => m.Stock)
                .Include(m => m.SideEffects)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (medicine == null)
            {
                TempData["MedicineError"] =
                    "Medicine not found.";

                return RedirectToAction(nameof(AddMedicine));
            }

            var form = new MedicineFormViewModel
            {
                Id = medicine.Id,
                Name = medicine.Name,
                CategoryId = medicine.CategoryId,
                ProductTypeId = medicine.ProductTypeId,
                Manufacturer = medicine.Manufacturer,
                Price = medicine.Price,

                StockQuantity =
                    medicine.Stock?.Quantity ?? 0,

                ExpiryDate =
                    medicine.Stock?.ExpiryDate ??
                    DateOnly.FromDateTime(
                        DateTime.UtcNow.AddYears(2)),

                RequiresPrescription =
                    medicine.RequiresPrescription,

                GenericName = medicine.GenericName,
                Unit = medicine.Unit,
                Description = medicine.Description,

                // NEW: existing Cloudinary URL is passed
                // into the edit form.
                ImageUrl = medicine.ImageUrl,

                SensitivityLevel =
                    medicine.SensitivityLevel,

                SideEffects = medicine.SideEffects
                    .Select(se =>
                        new SideEffectFormViewModel
                        {
                            Effect = se.Effect,
                            Severity = se.Severity
                        })
                    .ToList()
            };

            var model =
                new AdminMedicineFormPageViewModel
                {
                    Form = form,
                    CategoryOptions =
                        await BuildCategoryDropdownOptionsAsync(),
                    ProductTypeOptions =
                        await BuildProductTypeDropdownOptionsAsync()
                };

            return View("AddMedicine", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMedicine(
            MedicineFormViewModel form)
        {
            form.SideEffects = form.SideEffects
                .Where(se =>
                    !string.IsNullOrWhiteSpace(se.Effect))
                .ToList();

            if (form.Id is null)
            {
                TempData["MedicineError"] =
                    "Missing medicine id.";

                return RedirectToAction(nameof(AddMedicine));
            }

            string? finalImageUrl = form.ImageUrl;

            if (form.ImageFile != null &&
                form.ImageFile.Length > 0)
            {
                if (!IsValidImageFile(form.ImageFile))
                {
                    ModelState.AddModelError(
                        nameof(form.ImageFile),
                        "Upload a JPG, PNG, or WEBP image under 5MB.");
                }
                else
                {
                    try
                    {
                        finalImageUrl =
                            await _imageUploadService
                                .UploadMedicineImageAsync(
                                    form.ImageFile);
                    }
                    catch (InvalidOperationException ex)
                    {
                        ModelState.AddModelError(
                            nameof(form.ImageFile),
                            ex.Message);
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                var invalidModel =
                    new AdminMedicineFormPageViewModel
                    {
                        Form = form,
                        CategoryOptions =
                            await BuildCategoryDropdownOptionsAsync(),
                        ProductTypeOptions =
                            await BuildProductTypeDropdownOptionsAsync()
                    };

                return View("AddMedicine", invalidModel);
            }

            var medicine = await _db.Medicines
                .Include(m => m.Stock)
                .Include(m => m.SideEffects)
                .FirstOrDefaultAsync(m =>
                    m.Id == form.Id.Value);

            if (medicine == null)
            {
                TempData["MedicineError"] =
                    "Medicine not found — it may have been deleted.";

                return RedirectToAction(nameof(AddMedicine));
            }

            var categoryExists =
                await _db.Categories.AnyAsync(c =>
                    c.Id == form.CategoryId);

            var productTypeExists =
                await _db.ProductTypes.AnyAsync(p =>
                    p.Id == form.ProductTypeId);

            if (!categoryExists ||
                !productTypeExists)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The selected category or product type " +
                    "no longer exists — refresh the page.");

                var invalidModel =
                    new AdminMedicineFormPageViewModel
                    {
                        Form = form,
                        CategoryOptions =
                            await BuildCategoryDropdownOptionsAsync(),
                        ProductTypeOptions =
                            await BuildProductTypeDropdownOptionsAsync()
                    };

                return View("AddMedicine", invalidModel);
            }

            medicine.Name = form.Name.Trim();
            medicine.CategoryId = form.CategoryId;
            medicine.ProductTypeId = form.ProductTypeId;

            medicine.Manufacturer =
                string.IsNullOrWhiteSpace(form.Manufacturer)
                    ? null
                    : form.Manufacturer.Trim();

            medicine.GenericName =
                string.IsNullOrWhiteSpace(form.GenericName)
                    ? null
                    : form.GenericName.Trim();

            medicine.Unit =
                string.IsNullOrWhiteSpace(form.Unit)
                    ? null
                    : form.Unit.Trim();

            medicine.Description =
                string.IsNullOrWhiteSpace(form.Description)
                    ? null
                    : form.Description.Trim();

            medicine.ImageUrl =
                string.IsNullOrWhiteSpace(finalImageUrl)
                    ? null
                    : finalImageUrl.Trim();

            medicine.Price = form.Price;

            medicine.RequiresPrescription =
                form.RequiresPrescription;

            medicine.SensitivityLevel =
                string.IsNullOrWhiteSpace(
                    form.SensitivityLevel)
                    ? null
                    : form.SensitivityLevel;

            if (medicine.Stock != null)
            {
                medicine.Stock.Quantity =
                    form.StockQuantity;

                medicine.Stock.ExpiryDate =
                    form.ExpiryDate;

                medicine.Stock.UpdatedAt =
                    DateTime.UtcNow;
            }
            else
            {
                medicine.Stock = new Stock
                {
                    Quantity = form.StockQuantity,
                    ExpiryDate = form.ExpiryDate,
                    UpdatedAt = DateTime.UtcNow
                };
            }

            _db.SideEffects.RemoveRange(
                medicine.SideEffects);

            medicine.SideEffects =
                form.SideEffects
                    .Select(se => new SideEffect
                    {
                        Effect = se.Effect.Trim(),
                        Severity = se.Severity
                    })
                    .ToList();

            await _db.SaveChangesAsync();

            TempData["MedicineSuccess"] =
                $"Medicine '{medicine.Name}' updated.";

            // Keep the redirect fix you already made.
            return RedirectToAction(nameof(Medicines));
        }


        // =====================
        // Delete Medicine
        // =====================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMedicine(int id)
        {
            var medicine = await _db.Medicines
                .Include(m => m.OrderItems)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (medicine == null)
            {
                TempData["MedicineError"] =
                    "Medicine not found.";

                return RedirectToAction(nameof(AddMedicine));
            }

            if (medicine.OrderItems.Count > 0)
            {
                TempData["MedicineError"] =
                    $"Cannot delete '{medicine.Name}' — " +
                    $"it appears in {medicine.OrderItems.Count} " +
                    "past order(s). Deleting it would corrupt order history.";

                return RedirectToAction(
                    nameof(EditMedicine),
                    new { id = medicine.Id });
            }

            _db.Medicines.Remove(medicine);
            await _db.SaveChangesAsync();

            TempData["MedicineSuccess"] =
                $"Medicine '{medicine.Name}' deleted.";

            return RedirectToAction(nameof(AddMedicine));
        }


        // =====================
        // Helpers
        // =====================

        private async Task<AdminCategoriesViewModel>
            BuildCategoriesViewModelAsync()
        {
            var categories = await _db.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.SubCategories)
                .Include(c => c.Medicines)
                .OrderBy(c =>
                    c.ParentCategoryId == null ? 0 : 1)
                .ThenBy(c => c.Name)
                .ToListAsync();

            var productTypes = await _db.ProductTypes
                .Include(p => p.Medicines)
                .OrderBy(p => p.Name)
                .ToListAsync();

            return new AdminCategoriesViewModel
            {
                Categories = categories
                    .Select(c =>
                        new CategoryRowViewModel
                        {
                            Id = c.Id,
                            Name = c.Name,
                            Description = c.Description,
                            ParentCategoryId =
                                c.ParentCategoryId,
                            ParentCategoryName =
                                c.ParentCategory?.Name,
                            SubCategoryCount =
                                c.SubCategories.Count,
                            MedicineCount =
                                c.Medicines.Count
                        })
                    .ToList(),

                ProductTypes = productTypes
                    .Select(p =>
                        new ProductTypeRowViewModel
                        {
                            Id = p.Id,
                            Name = p.Name,
                            MedicineCount =
                                p.Medicines.Count
                        })
                    .ToList(),

                // Only top-level categories can be
                // selected as parents.
                ParentCategoryOptions = categories
                    .Where(c =>
                        c.ParentCategoryId == null)
                    .Select(c =>
                        new CategoryOptionViewModel
                        {
                            Id = c.Id,
                            Name = c.Name
                        })
                    .ToList()
            };
        }

        private async Task<bool> IsDescendantAsync(
            int categoryId,
            int candidateParentId)
        {
            int? currentId = candidateParentId;

            while (currentId.HasValue)
            {
                if (currentId.Value == categoryId)
                {
                    return true;
                }

                currentId = await _db.Categories
                    .Where(c =>
                        c.Id == currentId.Value)
                    .Select(c =>
                        c.ParentCategoryId)
                    .FirstOrDefaultAsync();
            }

            return false;
        }

        private async Task<List<DropdownOptionViewModel>>
            BuildCategoryDropdownOptionsAsync()
        {
            var categories = await _db.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            var byParent =
                categories.ToLookup(
                    c => c.ParentCategoryId);

            var options =
                new List<DropdownOptionViewModel>();

            void AddChildren(
                int? parentId,
                int depth)
            {
                foreach (var cat in
                    byParent[parentId]
                        .OrderBy(c => c.Name))
                {
                    var prefix =
                        depth == 0
                            ? ""
                            : new string('-', depth) + " ";

                    options.Add(
                        new DropdownOptionViewModel
                        {
                            Id = cat.Id,
                            Label =
                                prefix + cat.Name
                        });

                    AddChildren(
                        cat.Id,
                        depth + 1);
                }
            }

            AddChildren(null, 0);

            return options;
        }

        private async Task<List<DropdownOptionViewModel>>
            BuildProductTypeDropdownOptionsAsync()
        {
            return await _db.ProductTypes
                .OrderBy(p => p.Name)
                .Select(p =>
                    new DropdownOptionViewModel
                    {
                        Id = p.Id,
                        Label = p.Name
                    })
                .ToListAsync();
        }

        // Validates images before sending them
        // to Cloudinary.
        private static bool IsValidImageFile(
            IFormFile file)
        {
            var allowedTypes = new[]
            {
                "image/jpeg",
                "image/png",
                "image/webp"
            };

            const long maxBytes =
                5 * 1024 * 1024; // 5 MB

            return allowedTypes.Contains(
                       file.ContentType)
                   &&
                   file.Length <= maxBytes;
        }
    }
}
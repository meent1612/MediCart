using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(
            ApplicationDbContext db,
            Services.IImageUploadService imageUploadService,
            UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _imageUploadService = imageUploadService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var currentAdmin = await _userManager.GetUserAsync(User);

            // TODO(backend): once [Authorize(Roles = "Admin")] is turned on,
            // currentAdmin will never be null here. Until then, fall back to
            // an empty shell so the page doesn't crash for a logged-out visitor.
            if (currentAdmin == null)
            {
                return View(new AdminProfileViewModel());
            }

            var roles = await _userManager.GetRolesAsync(currentAdmin);

            var nameParts = currentAdmin.FullName
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var initials = nameParts.Length >= 2
                ? $"{nameParts[0][0]}{nameParts[1][0]}".ToUpperInvariant()
                : currentAdmin.FullName.Length >= 2
                    ? currentAdmin.FullName.Substring(0, 2).ToUpperInvariant()
                    : currentAdmin.FullName.ToUpperInvariant();

            var recentActivity = await _db.AuditLogs
                .Where(a => a.AdminId == currentAdmin.Id)
                .OrderByDescending(a => a.CreatedAt)
                .Take(8)
                .Select(a => new AdminActivityItemViewModel
                {
                    Action = a.Action,
                    TableName = a.TableName,
                    RecordId = a.RecordId,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();

            var model = new AdminProfileViewModel
            {
                FullName = currentAdmin.FullName,
                Email = currentAdmin.Email ?? "",
                PhoneNumber = currentAdmin.PhoneNumber,
                Initials = initials,
                RoleDisplay = roles.FirstOrDefault() ?? "Admin",
                RecentActivity = recentActivity
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(AdminProfileFormViewModel form)
        {
            if (!ModelState.IsValid)
            {
                TempData["ProfileError"] = "Please enter a valid name and phone number.";
                return RedirectToAction(nameof(Profile));
            }

            var currentAdmin = await _userManager.GetUserAsync(User);
            if (currentAdmin == null)
            {
                TempData["ProfileError"] = "You need to be logged in to update your profile.";
                return RedirectToAction(nameof(Profile));
            }

            currentAdmin.FullName = form.FullName.Trim();
            currentAdmin.PhoneNumber = string.IsNullOrWhiteSpace(form.PhoneNumber)
                ? null
                : form.PhoneNumber.Trim();

            var result = await _userManager.UpdateAsync(currentAdmin);

            TempData["ProfileSuccess"] = result.Succeeded
                ? "Profile updated."
                : null;

            if (!result.Succeeded)
            {
                TempData["ProfileError"] = string.Join(" ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction(nameof(Profile));
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
public async Task<IActionResult> CreateCategory(CategoryFormViewModel form)
{
    if (!ModelState.IsValid)
    {
        TempData["CategoryError"] = ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .FirstOrDefault() ?? "Please enter a valid name.";

        return RedirectToAction(nameof(Categories));
    }

    var name = form.Name.Trim();

    if (form.ParentCategoryId is null)
    {
        var duplicate = await _db.Categories.AnyAsync(c =>
            c.Name.ToLower() == name.ToLower());

        if (duplicate)
        {
            TempData["CategoryError"] = $"'{name}' already exists as a category.";
            return RedirectToAction(nameof(Categories));
        }

        _db.Categories.Add(new Category
        {
            Name = name,
            Description = string.IsNullOrWhiteSpace(form.Description) ? null : form.Description.Trim(),
            CreatedAt = DateTime.UtcNow
        });

        TempData["CategorySuccess"] = $"Category '{name}' added.";
    }
    else
    {
        var parentExists = await _db.Categories.AnyAsync(c => c.Id == form.ParentCategoryId.Value);
        if (!parentExists)
        {
            TempData["CategoryError"] = "Selected parent category no longer exists — refresh the page.";
            return RedirectToAction(nameof(Categories));
        }

        var duplicate = await _db.SubCategories.AnyAsync(sc =>
            sc.CategoryId == form.ParentCategoryId.Value &&
            sc.Name.ToLower() == name.ToLower());

        if (duplicate)
        {
            TempData["CategoryError"] = $"'{name}' already exists under this category.";
            return RedirectToAction(nameof(Categories));
        }

        _db.SubCategories.Add(new SubCategory
        {
            CategoryId = form.ParentCategoryId.Value,
            Name = name,
            Description = string.IsNullOrWhiteSpace(form.Description) ? null : form.Description.Trim(),
            CreatedAt = DateTime.UtcNow
        });

        TempData["CategorySuccess"] = $"Subcategory '{name}' added.";
    }

    await _db.SaveChangesAsync();
    return RedirectToAction(nameof(Categories));
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> EditCategory(CategoryFormViewModel form)
{
    if (form.Id is null || string.IsNullOrEmpty(form.Kind) || !ModelState.IsValid)
    {
        TempData["CategoryError"] = ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .FirstOrDefault() ?? "Please enter a valid name.";

        return RedirectToAction(nameof(Categories));
    }

    var name = form.Name.Trim();

    // Editing can't move a row between the two tables — block it explicitly
    // rather than silently doing the wrong thing.
    var wouldChangeKind = form.Kind == "category"
        ? form.ParentCategoryId.HasValue
        : !form.ParentCategoryId.HasValue;

    if (wouldChangeKind)
    {
        TempData["CategoryError"] =
            "Can't turn a category into a subcategory (or back) by editing. Delete it and add it again instead.";
        return RedirectToAction(nameof(Categories));
    }

    if (form.Kind == "category")
    {
        var category = await _db.Categories.FindAsync(form.Id.Value);
        if (category == null)
        {
            TempData["CategoryError"] = "Category not found.";
            return RedirectToAction(nameof(Categories));
        }

        var duplicate = await _db.Categories.AnyAsync(c =>
            c.Id != category.Id && c.Name.ToLower() == name.ToLower());

        if (duplicate)
        {
            TempData["CategoryError"] = $"'{name}' already exists as a category.";
            return RedirectToAction(nameof(Categories));
        }

        category.Name = name;
        category.Description = string.IsNullOrWhiteSpace(form.Description) ? null : form.Description.Trim();
    }
    else
    {
        var subCategory = await _db.SubCategories.FindAsync(form.Id.Value);
        if (subCategory == null)
        {
            TempData["CategoryError"] = "Subcategory not found.";
            return RedirectToAction(nameof(Categories));
        }

        var duplicate = await _db.SubCategories.AnyAsync(sc =>
            sc.Id != subCategory.Id &&
            sc.CategoryId == form.ParentCategoryId!.Value &&
            sc.Name.ToLower() == name.ToLower());

        if (duplicate)
        {
            TempData["CategoryError"] = $"'{name}' already exists under this category.";
            return RedirectToAction(nameof(Categories));
        }

                subCategory.Name = name;
        subCategory.Description = string.IsNullOrWhiteSpace(form.Description) ? null : form.Description.Trim();
        subCategory.CategoryId = form.ParentCategoryId!.Value;
    }

    await _db.SaveChangesAsync();
    TempData["CategorySuccess"] = $"'{name}' updated.";
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
            $"Cannot delete '{category.Name}' — it has {category.SubCategories.Count} " +
            $"subcategor{(category.SubCategories.Count == 1 ? "y" : "ies")}. Delete those first.";
        return RedirectToAction(nameof(Categories));
    }

    if (category.Medicines.Count > 0)
    {
        TempData["CategoryError"] =
            $"Cannot delete '{category.Name}' — {category.Medicines.Count} medicine(s) still use it.";
        return RedirectToAction(nameof(Categories));
    }

    _db.Categories.Remove(category);
    await _db.SaveChangesAsync();

    TempData["CategorySuccess"] = $"Category '{category.Name}' deleted.";
    return RedirectToAction(nameof(Categories));
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> DeleteSubCategory(int id)
{
    var subCategory = await _db.SubCategories
        .Include(sc => sc.Medicines)
        .FirstOrDefaultAsync(sc => sc.Id == id);

    if (subCategory == null)
    {
        TempData["CategoryError"] = "Subcategory not found.";
        return RedirectToAction(nameof(Categories));
    }

    if (subCategory.Medicines.Count > 0)
    {
        TempData["CategoryError"] =
            $"Cannot delete '{subCategory.Name}' — {subCategory.Medicines.Count} medicine(s) still use it.";
        return RedirectToAction(nameof(Categories));
    }

    _db.SubCategories.Remove(subCategory);
    await _db.SaveChangesAsync();

    TempData["CategorySuccess"] = $"Subcategory '{subCategory.Name}' deleted.";
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

        private async Task<AdminCategoriesViewModel> BuildCategoriesViewModelAsync()
{
    var categories = await _db.Categories
        .Include(c => c.SubCategories)
        .Include(c => c.Medicines)
        .OrderBy(c => c.Name)
        .ToListAsync();

    var subCategories = await _db.SubCategories
        .Include(sc => sc.Category)
        .Include(sc => sc.Medicines)
        .OrderBy(sc => sc.Name)
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
            SubCategoryCount = c.SubCategories.Count,
            MedicineCount = c.Medicines.Count
        }).ToList(),

        SubCategories = subCategories.Select(sc => new SubCategoryRowViewModel
        {
            Id = sc.Id,
            Name = sc.Name,
            Description = sc.Description,
            CategoryId = sc.CategoryId,
            CategoryName = sc.Category.Name,
            MedicineCount = sc.Medicines.Count
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

private async Task<List<DropdownOptionViewModel>> BuildCategoryDropdownOptionsAsync()
{
    return await _db.Categories
        .OrderBy(c => c.Name)
        .Select(c => new DropdownOptionViewModel
        {
            Id = c.Id,
            Label = c.Name
        })
        .ToListAsync();
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
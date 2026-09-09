using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MediCart.Web.Data;
using MediCart.Web.Models;
using MediCart.Web.Services;

namespace MediCart.Web.Controllers
{
    public class MedicinesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICartService _cartService;

        public MedicinesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ICartService cartService)
        {
            _context = context;
            _userManager = userManager;
            _cartService = cartService;
        }

        public async Task<IActionResult> Index()
        {
            var medicines = await _context.Medicines
                .Include(m => m.Category)
                .Include(m => m.SubCategory)
                .Include(m => m.ProductType)
                .Include(m => m.Stock)
                .Include(m => m.SideEffects)
                .OrderBy(m => m.Name)
                .ToListAsync();

            var viewModels = medicines.Select(m => new MedicineViewModel
            {
                Id = m.Id,
                Name = m.Name,
                Composition = m.GenericName ?? "",
                Manufacturer = m.Manufacturer ?? "",
                ProductTypeId = m.ProductTypeId,
                ProductType = m.ProductType?.Name ?? "",
                CategoryId = m.CategoryId,
                Category = m.Category?.Name ?? "",
                SubCategoryId = m.SubCategoryId,
                SubCategory = m.SubCategory?.Name,
                Price = m.Price,
                Stock = m.Stock?.Quantity ?? 0,
                Unit = m.Unit,
                ExpiryDate = m.Stock?.ExpiryDate,
                RequiresRx = m.RequiresPrescription,
                Description = m.Description ?? "",
                Dosage = m.Dosage,
                ImageUrl = m.ImageUrl,
                SideEffects = m.SideEffects.Select(se => new SideEffectViewModel
                {
                    Effect = se.Effect,
                    Severity = se.Severity
                }).ToList()
            }).ToList();

            var categories = await _context.Categories
                .Include(c => c.SubCategories)
                .OrderBy(c => c.Name)
                .ToListAsync();

            ViewBag.Categories = categories.Select(c => new CategoryFilterOption
            {
                Id = c.Id,
                Name = c.Name,
                SubCategories = c.SubCategories
                    .OrderBy(sc => sc.Name)
                    .Select(sc => new SubCategoryFilterOption { Id = sc.Id, Name = sc.Name })
                    .ToList()
            }).ToList();

            ViewBag.ProductTypes = await _context.ProductTypes
                .OrderBy(pt => pt.Name)
                .Select(pt => new ProductTypeFilterOption { Id = pt.Id, Name = pt.Name })
                .ToListAsync();

            var cartQuantities = new Dictionary<int, int>();
            var userId = _userManager.GetUserId(User);

            if (userId != null && User.IsInRole("Customer"))
            {
                var cartItems = await _context.CartItems
                    .Where(ci => ci.UserId == userId)
                    .ToListAsync();

                cartQuantities = cartItems.ToDictionary(
                    ci => ci.MedicineId,
                    ci => ci.Quantity);
            }

            ViewBag.CartQuantities = cartQuantities;

            return View(viewModels);
        }
    }
}
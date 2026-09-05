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
                RequiresRx = m.RequiresPrescription,
                About = m.Description ?? "",
                ImageUrl = m.ImageUrl,
                SideEffects = m.SideEffects.Select(se => se.Effect).ToList(),
                Strength = null,
                Dosage = null,
                Potency = null,
                Popularity = null,
                UseTags = new()
            }).ToList();

            // Full filter option lists straight from the DB — independent of
            // which medicines happen to be loaded above, so a category/type
            // with zero matching medicines right now still appears as a
            // selectable filter (matching zero results, which is correct).
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

            // For logged-in customers, pass a dictionary of MedicineId → quantity
            // already in their cart so the view can show "in cart" state on each card.
            // For guests and admins this is an empty dictionary.
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

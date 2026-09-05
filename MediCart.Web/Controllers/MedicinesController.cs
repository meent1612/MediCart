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
                ProductType = m.ProductType?.Name ?? "",
                Category = m.Category?.Name ?? "",
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
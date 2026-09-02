using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MediCart.Web.Data;
using MediCart.Web.Models;

namespace MediCart.Web.Controllers
{
    public class MedicinesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MedicinesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Pulls every medicine added via the Admin panel, with its
            // Category, ProductType, Stock, and SideEffects loaded.
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

                // TODO(backend): Strength, Dosage, Potency, Popularity, and
                // UseTags have no matching columns in the Medicine table yet.
                // Left null/empty on purpose (see MedicineViewModel.cs) so the
                // UI can show them as "not available" instead of faking data.
                Strength = null,
                Dosage = null,
                Potency = null,
                Popularity = null,
                UseTags = new()
            }).ToList();

            return View(viewModels);
        }
    }
}

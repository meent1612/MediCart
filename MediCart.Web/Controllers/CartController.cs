using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MediCart.Web.Models;

namespace MediCart.Web.Controllers
{
    public class CartController : Controller
    {
        public IActionResult Index()
        {
            // TODO(backend): replace with the signed-in user's actual cart
            // once cart persistence (session or database) is wired up.
            var items = new List<CartItemViewModel>
            {
                new CartItemViewModel
                {
                    Id = 1, Name = "Seclo 20", Description = "Omeprazole 20mg · strip of 10",
                    IconType = "tablet", RequiresRx = true, StockStatus = "In stock",
                    UnitPrice = 60, Quantity = 2
                },
                new CartItemViewModel
                {
                    Id = 2, Name = "Ambrox Syrup", Description = "Ambroxol 15mg/5ml · 100ml bottle",
                    IconType = "bottle", RequiresRx = false, StockStatus = "In stock",
                    UnitPrice = 85, Quantity = 1
                },
                new CartItemViewModel
                {
                    Id = 3, Name = "Napa Extra", Description = "Paracetamol 500mg + Caffeine · strip of 12",
                    IconType = "tablet", RequiresRx = false, StockStatus = "In stock",
                    UnitPrice = 30, Quantity = 3
                },
            };

            return View(items);
        }
    }
}

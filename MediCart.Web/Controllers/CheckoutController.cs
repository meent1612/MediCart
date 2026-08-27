using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MediCart.Web.Models;

namespace MediCart.Web.Controllers
{
    public class CheckoutController : Controller
    {
        public IActionResult Index()
        {
            // TODO(backend): pull the real cart (session/DB) instead of this
            // seed list once cart persistence is shared across pages.
            var model = new CheckoutViewModel
            {
                Items = new List<CheckoutLineItemViewModel>
                {
                    new CheckoutLineItemViewModel { Id = 1, Name = "Seclo 20", Quantity = 2, UnitPrice = 60, RequiresRx = true },
                    new CheckoutLineItemViewModel { Id = 2, Name = "Ambrox Syrup", Quantity = 1, UnitPrice = 85, RequiresRx = false },
                    new CheckoutLineItemViewModel { Id = 3, Name = "Napa Extra", Quantity = 3, UnitPrice = 30, RequiresRx = false },
                }
            };

            return View(model);
        }
    }
}

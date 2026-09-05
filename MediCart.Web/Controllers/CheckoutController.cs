using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MediCart.Web.Data;
using MediCart.Web.Models;
using MediCart.Web.Services;

namespace MediCart.Web.Controllers
{
    // Only logged-in customers should ever reach the checkout page.
    // This also stops a guest from skipping the Add-to-cart guard by
    // typing /Checkout straight into the address bar.
    [Authorize(Roles = "Customer")]
    public class CheckoutController : Controller
    {
        private readonly ICartService _cartService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CheckoutController(
            ICartService cartService,
            UserManager<ApplicationUser> userManager)
        {
            _cartService = cartService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;
            var cartItems = await _cartService.GetCartAsync(userId);

            // Nothing to check out — send them back to the cart instead of
            // showing an empty checkout page.
            if (cartItems.Count == 0)
            {
                return RedirectToAction("Index", "Cart");
            }

            var model = new CheckoutViewModel
            {
                Items = cartItems.Select(ci => new CheckoutLineItemViewModel
                {
                    Id = ci.MedicineId,
                    Name = ci.Name,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.UnitPrice,
                    RequiresRx = ci.RequiresRx
                }).ToList()
            };

            return View(model);
        }
    }
}
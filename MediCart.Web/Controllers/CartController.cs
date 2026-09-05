using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MediCart.Web.Data;
using MediCart.Web.Services;

namespace MediCart.Web.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(
            ICartService cartService,
            UserManager<ApplicationUser> userManager)
        {
            _cartService = cartService;
            _userManager = userManager;
        }

        // GET /Cart/Index
        // Shows the customer's full cart.
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;
            var items = await _cartService.GetCartAsync(userId);
            return View(items);
        }

        // POST /Cart/Add
        // Called by the JS on the medicine browse page and detail page.
        // Returns JSON so the page does not reload.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int medicineId, int quantity = 1)
        {
            var userId = _userManager.GetUserId(User)!;
            var result = await _cartService.AddToCartAsync(userId, medicineId, quantity);

            if (!result.Success)
                return BadRequest(new { error = result.ErrorMessage });

            return Ok(new
            {
                newQuantity = result.NewQuantity,
                newStockQuantity = result.NewStockQuantity,
                cartItemCount = result.CartItemCount
            });
        }

        // POST /Cart/UpdateQuantity
        // Called by the +/- buttons in the cart view.
        // Returns JSON so the page does not reload.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int newQuantity)
        {
            var userId = _userManager.GetUserId(User)!;
            var result = await _cartService.UpdateQuantityAsync(userId, cartItemId, newQuantity);

            if (!result.Success)
                return BadRequest(new { error = result.ErrorMessage });

            return Ok(new
            {
                newQuantity = result.NewQuantity,
                newStockQuantity = result.NewStockQuantity,
                cartItemCount = result.CartItemCount
            });
        }

        // POST /Cart/Remove
        // Called by the remove button in the cart view.
        // Returns JSON so the page does not reload.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int cartItemId)
        {
            var userId = _userManager.GetUserId(User)!;
            var result = await _cartService.RemoveItemAsync(userId, cartItemId);

            if (!result.Success)
                return BadRequest(new { error = result.ErrorMessage });

            return Ok(new
            {
                cartItemCount = result.CartItemCount
            });
        }

        // GET /Cart/Count
        // Called by the navbar JS to update the cart badge without reloading.
        // Returns a plain integer as JSON.
        [HttpGet]
        public async Task<IActionResult> Count()
        {
            var userId = _userManager.GetUserId(User)!;
            var count = await _cartService.GetCartItemCountAsync(userId);
            return Ok(new { cartItemCount = count });
        }
    }
}
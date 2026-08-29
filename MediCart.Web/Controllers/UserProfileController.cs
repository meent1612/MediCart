using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MediCart.Web.Data;
using MediCart.Web.Models;

namespace MediCart.Web.Controllers
{
    [Authorize]
    public class UserProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserProfileController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var model = new UserProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,

                // TODO: replace with real query once Orders table exists, e.g.:
                // Orders = _db.Orders.Where(o => o.UserId == user.Id).ToList()
                Orders = GetSampleOrders()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(UserProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (!ModelState.IsValid)
            {
                model.Orders = GetSampleOrders();
                return View(model);
            }

            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["ProfileSuccess"] = "Your profile has been updated.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            model.Orders = GetSampleOrders();
            return View(model);
        }

        private static List<OrderHistoryItem> GetSampleOrders()
        {
            return new List<OrderHistoryItem>
            {
                new() { OrderId = "MC-10482", Date = new DateTime(2026, 8, 22), ItemCount = 3, Total = 355, Status = "Shipped" },
                new() { OrderId = "MC-10331", Date = new DateTime(2026, 8, 14), ItemCount = 1, Total = 60, Status = "Delivered" },
                new() { OrderId = "MC-10298", Date = new DateTime(2026, 8, 2), ItemCount = 4, Total = 410, Status = "Pending review" },
                new() { OrderId = "MC-10120", Date = new DateTime(2026, 7, 19), ItemCount = 2, Total = 175, Status = "Rejected" },
            };
        }
    }
}
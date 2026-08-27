using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Authorization; // uncomment once backend adds admin role/policy

namespace MediCart.Controllers
{
    // [Authorize(Roles = "Admin")] // ask backend teammate for the exact policy/role name
    public class AdminController : Controller
    {
        [HttpGet]
        public IActionResult Profile()
        {
            return View();
        }

        // TEMP placeholder stubs so sidebar links don't 404 — replace each
        // with a real action + view as those pages get built.
        [HttpGet] public IActionResult Dashboard() => View("ComingSoon");
        [HttpGet] public IActionResult Medicines() => View("ComingSoon");
        [HttpGet] public IActionResult Categories() => View("ComingSoon");
        [HttpGet] public IActionResult IncomingOrders() => View("ComingSoon");
        [HttpGet] public IActionResult FlaggedOrders() => View("ComingSoon");
        [HttpGet] public IActionResult StockExpiry() => View("ComingSoon");
        [HttpGet] public IActionResult AuditLog() => View("ComingSoon");
        [HttpGet] public IActionResult ContactMessages() => View("ComingSoon");
    }
}

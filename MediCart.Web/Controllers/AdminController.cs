using Microsoft.AspNetCore.Mvc;

namespace MediCart.Controllers
{
    public class AdminController : Controller
    {
        // Temporary – just returns the static profile view
        public IActionResult Profile()
        {
            return View();
        }
    }
}
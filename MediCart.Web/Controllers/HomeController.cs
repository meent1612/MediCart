using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MediCart.Web.Models;

namespace MediCart.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    public IActionResult About()
    {
       return View();
    }
    [HttpGet]
    public IActionResult Contact()
    {
       return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Contact(ContactViewModel model)
    {
      if (!ModelState.IsValid)
    {
        return View(model);
    }

    TempData["ContactSuccess"] = "Thanks — we've received your message and will get back to you within 24 hours.";
     return RedirectToAction(nameof(Contact));
   }
   public IActionResult Terms()
   {
      return View();
   }

}

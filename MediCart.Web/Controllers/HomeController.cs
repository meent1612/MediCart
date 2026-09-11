using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MediCart.Web.Data;
using MediCart.Web.Models;

namespace MediCart.Web.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _context.Categories
            .Include(c => c.SubCategories)
            .OrderBy(c => c.Name)
            .ToListAsync();

        ViewBag.Categories = categories.Select(c => new CategoryFilterOption
        {
            Id = c.Id,
            Name = c.Name,
            SubCategories = c.SubCategories
                .OrderBy(sc => sc.Name)
                .Select(sc => new SubCategoryFilterOption { Id = sc.Id, Name = sc.Name })
                .ToList()
        }).ToList();

        ViewBag.ProductTypes = await _context.ProductTypes
            .OrderBy(pt => pt.Name)
            .Select(pt => new ProductTypeFilterOption { Id = pt.Id, Name = pt.Name })
            .ToListAsync();

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

    // POST /Home/Contact
    // Guest/Customer submission — no login required (Report 03 §1.12).
    // ContactMessage entity has no Subject column, so Subject is folded
    // into the stored Message text rather than dropped.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Contact(ContactViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var fullMessage = string.IsNullOrWhiteSpace(model.Subject)
            ? model.Message.Trim()
            : $"[{model.Subject.Trim()}] {model.Message.Trim()}";

        _context.ContactMessages.Add(new ContactMessage
        {
            Name = model.FullName.Trim(),
            Email = model.Email.Trim(),
            Message = fullMessage,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        TempData["ContactSuccess"] = "Thanks — we've received your message and will get back to you within 24 hours.";
        return RedirectToAction(nameof(Contact));
    }

    public IActionResult Terms()
    {
       return View();
    }

    [HttpGet]
    public IActionResult Loading()
    {
        return View();
    }
}
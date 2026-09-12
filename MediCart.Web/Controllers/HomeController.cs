using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MediCart.Web.Data;
using MediCart.Web.Models;

namespace MediCart.Web.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
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
    // If the submitter is logged in, UserId is stamped so it shows up
    // under their profile's "My messages" (Step 3 follow-up, #5).
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

        string? userId = null;
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            userId = _userManager.GetUserId(User);
        }

        _context.ContactMessages.Add(new ContactMessage
        {
            Name = model.FullName.Trim(),
            Email = model.Email.Trim(),
            Message = fullMessage,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            UserId = userId
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
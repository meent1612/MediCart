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
        // Full filter option lists straight from the DB — same shape
        // MedicinesController uses, so the homepage cards link straight
        // into Medicines with the same query params the filters expect.
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

    [HttpGet]
    public IActionResult Loading()
    {
        return View();
    }
}

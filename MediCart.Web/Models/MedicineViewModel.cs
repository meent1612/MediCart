using System;
using System.Collections.Generic;

namespace MediCart.Web.Models
{
    public class SideEffectViewModel
    {
        public string Effect { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty; // "Mild" | "Moderate" | "High"
    }

    public class MedicineViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Composition { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;

        public int ProductTypeId { get; set; }
        public string ProductType { get; set; } = string.Empty;

        public int CategoryId { get; set; }
        public string Category { get; set; } = string.Empty;

        public int? SubCategoryId { get; set; }
        public string? SubCategory { get; set; }

        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string? Unit { get; set; }
        public DateOnly? ExpiryDate { get; set; }   // <-- DateOnly?, matches Stock.cs

        public bool RequiresRx { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? Dosage { get; set; }

        public string? ImageUrl { get; set; }
        public List<SideEffectViewModel> SideEffects { get; set; } = new();

        // SensitivityLevel is intentionally NOT a property here — never map
        // it onto this ViewModel. Only the Admin ViewModel should read
        // Medicine.SensitivityLevel.

        public string StockStatus =>
            Stock <= 0 ? "Out of stock" : Stock <= 10 ? "Low stock" : "In stock";

        public string StockCssClass =>
            Stock <= 0 ? "out" : Stock <= 10 ? "low" : "in";

        public bool IsExpired =>
            ExpiryDate.HasValue && ExpiryDate.Value < DateOnly.FromDateTime(DateTime.UtcNow);

        public bool IsExpiringSoon =>
            ExpiryDate.HasValue && !IsExpired &&
            ExpiryDate.Value.DayNumber - DateOnly.FromDateTime(DateTime.UtcNow).DayNumber <= 90;
    }
}
using MediCart.Web.Services;

namespace MediCart.Web.Models
{
    public class SideEffectViewModel
    {
        public string Effect { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
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
        public DateOnly? ExpiryDate { get; set; }

        public bool RequiresRx { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? Dosage { get; set; }

        public string? ImageUrl { get; set; }
        public List<SideEffectViewModel> SideEffects { get; set; } = new();

        // Days until expiry — 9999 when no expiry date is set
        public int DaysUntilExpiry =>
            ExpiryDate.HasValue
                ? ExpiryDate.Value.DayNumber - DateOnly.FromDateTime(DateTime.UtcNow).DayNumber
                : 9999;

        // Expiry state — uses StockExpiryHelper constants
        public bool IsExpired        => StockExpiryHelper.IsExpired(DaysUntilExpiry);
        public bool IsCriticalExpiry => StockExpiryHelper.IsCriticalExpiry(DaysUntilExpiry);
        public bool IsWarningExpiry  => StockExpiryHelper.IsWarningExpiry(DaysUntilExpiry);

        // Stock state
        public bool IsOutOfStock => StockExpiryHelper.IsOutOfStock(Stock);
        public bool IsLowStock   => StockExpiryHelper.IsLowStock(Stock);

        // Blocked from cart: expired or critical expiry
        public bool IsBlockedFromCart =>
            StockExpiryHelper.IsBlockedFromCart(DaysUntilExpiry) || IsOutOfStock;

        // Display helpers used by the browse page cards and badges
        public string StockStatus =>
            Stock <= 0      ? "Out of stock" :
            IsLowStock      ? "Low stock"    : "In stock";

        public string StockCssClass =>
            Stock <= 0  ? "out" :
            IsLowStock  ? "low" : "in";
    }
}
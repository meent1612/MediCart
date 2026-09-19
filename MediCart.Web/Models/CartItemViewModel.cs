using MediCart.Web.Services;

namespace MediCart.Web.Models
{
    public class CartItemViewModel
    {
        public int Id { get; set; }
        public int MedicineId { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string IconType { get; set; } = "tablet";
        public string? ImageUrl { get; set; }
        public bool RequiresRx { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public int AvailableStock { get; set; }
        public DateTime AddedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Raw days value — negative means already expired.
        // Set by CartService.GetCartAsync() using StockExpiryHelper.DaysUntilExpiry().
        public int DaysUntilExpiry { get; set; } = 9999;

        // Expiry state — derived from DaysUntilExpiry
        public bool IsExpired        => StockExpiryHelper.IsExpired(DaysUntilExpiry);
        public bool IsCriticalExpiry => StockExpiryHelper.IsCriticalExpiry(DaysUntilExpiry);
        public bool IsWarningExpiry  => StockExpiryHelper.IsWarningExpiry(DaysUntilExpiry);

        // Stock state
        public bool IsOutOfStock => StockExpiryHelper.IsOutOfStock(AvailableStock);
        public bool IsLowStock   => StockExpiryHelper.IsLowStock(AvailableStock);

        // Blocked from checkout: expired OR critical expiry OR out of stock
        public bool IsBlockedFromCheckout =>
            StockExpiryHelper.IsBlockedFromCart(DaysUntilExpiry) ||
            IsOutOfStock;

        public string StockStatus =>
            AvailableStock <= 0 ? "Out of stock" :
            IsLowStock          ? "Low stock"    : "In stock";

        public string StockCssClass =>
            AvailableStock <= 0 ? "out" :
            IsLowStock          ? "low" : "in";

        public decimal LineTotal => UnitPrice * Quantity;
    }
}
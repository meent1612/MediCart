namespace MediCart.Web.Models
{
    public class AdminStockExpiryViewModel
    {
        // "All" | "Warning" | "CriticalExpired" | "LowStock"
        public string Filter { get; set; } = "All";

        public int TotalCount { get; set; }
        public int WarningCount { get; set; }           // > 7 and <= 30 days
        public int CriticalExpiredCount { get; set; }   // <= 7 days OR already expired
        public int LowStockCount { get; set; }          // quantity <= 10 (includes 0)

        public List<AdminStockExpiryRowViewModel> Items { get; set; } = new();
    }

    public class AdminStockExpiryRowViewModel
    {
        public int MedicineId { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public string GenericName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateOnly? ExpiryDate { get; set; }
        public int DaysUntilExpiry { get; set; }        // negative = expired
        public int ProgressPercent { get; set; }
        public string ProgressColor { get; set; } = "green";

        // Stock badge — null Label means no stock issue (normal stock)
        public string? StockBadgeLabel { get; set; }
        public string? StockBadgeSeverity { get; set; }  // "danger" | "warning" | null

        // Expiry badge — null Label means no expiry issue
        public string? ExpiryBadgeLabel { get; set; }
        public string? ExpiryBadgeSeverity { get; set; } // "danger" | "warning" | null
    }
}
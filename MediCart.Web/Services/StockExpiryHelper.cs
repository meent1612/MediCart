namespace MediCart.Web.Services
{
    // Single source of truth for all stock and expiry thresholds.
    // Every controller, service, and ViewModel computed property must
    // use these constants — never hard-code the numbers anywhere else.
    //
    // Stock:
    //   Out of stock  = Quantity == 0
    //   Low stock     = Quantity <= 10  (includes 0)
    //
    // Expiry:
    //   Expired         = DaysUntilExpiry < 0
    //   Critical expiry = DaysUntilExpiry >= 0 && <= 7
    //   Warning expiry  = DaysUntilExpiry > 7  && <= 30
    //   Normal          = DaysUntilExpiry > 30

    public static class StockExpiryHelper
    {
        // ── Thresholds ────────────────────────────────────────────────────
        public const int LowStockThreshold    = 10;
        public const int CriticalExpiryDays   = 7;
        public const int WarningExpiryDays    = 30;

        // ── Stock helpers ─────────────────────────────────────────────────

        public static bool IsOutOfStock(int quantity)
            => quantity == 0;

        public static bool IsLowStock(int quantity)
            => quantity <= LowStockThreshold;   // includes 0

        // ── Expiry helpers ────────────────────────────────────────────────

        // daysUntilExpiry = ExpiryDate.DayNumber - today.DayNumber
        // Negative means already expired.

        public static bool IsExpired(int daysUntilExpiry)
            => daysUntilExpiry < 0;

        // Critical: not yet expired but <= 7 days left.
        public static bool IsCriticalExpiry(int daysUntilExpiry)
            => daysUntilExpiry >= 0 && daysUntilExpiry <= CriticalExpiryDays;

        // Warning: more than 7 days but <= 30 days left.
        public static bool IsWarningExpiry(int daysUntilExpiry)
            => daysUntilExpiry > CriticalExpiryDays && daysUntilExpiry <= WarningExpiryDays;

        // Blocked from cart: expired OR critical (customer cannot buy).
        public static bool IsBlockedFromCart(int daysUntilExpiry)
            => daysUntilExpiry <= CriticalExpiryDays;   // covers negative too

        // ── Badge factories ───────────────────────────────────────────────
        // Returns (label, severity) where severity is "danger" | "warning" | "success" | null.
        // Returns null label when no badge applies for that dimension.

        public static (string? Label, string? Severity) GetStockBadge(int quantity)
        {
            if (quantity == 0)
                return ("Out of stock", "danger");

            if (quantity <= LowStockThreshold)
                return ("Low stock", "warning");

            return (null, null);    // normal stock — no badge needed
        }

        public static (string? Label, string? Severity) GetExpiryBadge(int daysUntilExpiry)
        {
            if (daysUntilExpiry < 0)
                return ("Expired", "danger");

            if (daysUntilExpiry <= CriticalExpiryDays)
                return ("Critical — expiring soon", "danger");

            if (daysUntilExpiry <= WarningExpiryDays)
                return ($"Expiring in {daysUntilExpiry} days", "warning");

            return (null, null);    // normal — no badge needed
        }

        // ── Days calculator ───────────────────────────────────────────────
        // Call this once per row; pass the result to every helper above.

        public static int DaysUntilExpiry(DateOnly expiryDate)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            return expiryDate.DayNumber - today.DayNumber;
        }
    }
}
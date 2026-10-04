using System.Text.Json;
using System.Text.Json.Nodes;
using MediCart.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace MediCart.Web.Services.Ai.Tools
{
    public class AdminAttentionSummaryTool : IAiTool
    {
        private const int MaxListItems = 8;

        private readonly ApplicationDbContext _db;

        public AdminAttentionSummaryTool(ApplicationDbContext db)
        {
            _db = db;
        }

        public string Name => "get_admin_attention_summary";

        public string Description =>
            "Summary of pending and flagged orders, low and out-of-stock items, and expiring medicines.";

        public JsonObject ParametersSchema => new()
        {
            ["type"] = "object",
            ["properties"] = new JsonObject()
        };

        public IReadOnlyCollection<string> AllowedRoles => new[] { "Admin" };

        public async Task<string> ExecuteAsync(JsonObject arguments, AiCallContext context, CancellationToken ct)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var criticalLimit = today.AddDays(StockExpiryHelper.CriticalExpiryDays);
            var warningLimit = today.AddDays(StockExpiryHelper.WarningExpiryDays);

            // Orders
            var pendingOrders = await _db.Orders.CountAsync(o => o.Status == "Pending", ct);

            var flaggedQuery = _db.Orders.Where(o => o.IsFlagged && o.Status == "Pending");
            var flaggedCount = await flaggedQuery.CountAsync(ct);
            var flaggedRows = await flaggedQuery
                .OrderBy(o => o.CreatedAt)
                .Take(MaxListItems)
                .Select(o => new { o.Id, o.TotalAmount, o.CreatedAt })
                .ToListAsync(ct);

            // Stock
            var outOfStockCount = await _db.Stocks.CountAsync(s => s.Quantity == 0, ct);

            var lowStockQuery = _db.Stocks.Where(s => s.Quantity <= StockExpiryHelper.LowStockThreshold);
            var lowStockCount = await lowStockQuery.CountAsync(ct);
            var lowStockRows = await lowStockQuery
                .OrderBy(s => s.Quantity)
                .ThenBy(s => s.Medicine.Name)
                .Take(MaxListItems)
                .Select(s => new { s.Medicine.Name, s.Quantity })
                .ToListAsync(ct);

            // Expiry
            var expiredCount = await _db.Stocks.CountAsync(s => s.ExpiryDate < today, ct);
            var criticalCount = await _db.Stocks.CountAsync(
                s => s.ExpiryDate >= today && s.ExpiryDate <= criticalLimit, ct);
            var warningCount = await _db.Stocks.CountAsync(
                s => s.ExpiryDate > criticalLimit && s.ExpiryDate <= warningLimit, ct);

            var expiryRows = await _db.Stocks
                .Where(s => s.ExpiryDate <= warningLimit)
                .OrderBy(s => s.ExpiryDate)
                .Take(MaxListItems)
                .Select(s => new { s.Medicine.Name, s.ExpiryDate, s.Quantity })
                .ToListAsync(ct);

            var payload = new
            {
                asOfDateUtc = today.ToString("yyyy-MM-dd"),
                note = $"Counts are totals. Lists show at most {MaxListItems} items.",
                pendingOrdersAwaitingReview = pendingOrders,
                flaggedPendingOrders = new
                {
                    count = flaggedCount,
                    shown = flaggedRows.Select(o => new
                    {
                        orderNumber = "MC-" + (10000 + o.Id),
                        totalTaka = o.TotalAmount,
                        placedOn = o.CreatedAt.ToString("yyyy-MM-dd")
                    })
                },
                stock = new
                {
                    outOfStockCount,
                    lowStockCountIncludingOutOfStock = lowStockCount,
                    lowestShown = lowStockRows.Select(s => new
                    {
                        medicine = s.Name,
                        unitsLeft = s.Quantity
                    })
                },
                expiry = new
                {
                    expiredCount,
                    criticalWithin7DaysCount = criticalCount,
                    warningWithin30DaysCount = warningCount,
                    soonestShown = expiryRows.Select(s =>
                    {
                        var days = StockExpiryHelper.DaysUntilExpiry(s.ExpiryDate);
                        return new
                        {
                            medicine = s.Name,
                            expiryDate = s.ExpiryDate.ToString("yyyy-MM-dd"),
                            daysUntilExpiry = days,
                            status = DescribeExpiry(days),
                            unitsInStock = s.Quantity
                        };
                    })
                }
            };

            return JsonSerializer.Serialize(payload);
        }

        private static string DescribeExpiry(int daysUntilExpiry)
        {
            if (StockExpiryHelper.IsExpired(daysUntilExpiry)) return "expired";
            if (StockExpiryHelper.IsCriticalExpiry(daysUntilExpiry)) return "critical";
            return "warning";
        }
    }
}
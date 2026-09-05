using Microsoft.EntityFrameworkCore;
using MediCart.Web.Data;
using MediCart.Web.Models;

namespace MediCart.Web.Services
{
    // What the controller and background service talk to.
    // Never touches the DB directly — always goes through CartService.
    public interface ICartService
    {
        Task<List<CartItemViewModel>> GetCartAsync(string userId);
        Task<CartOperationResult> AddToCartAsync(string userId, int medicineId, int quantity);
        Task<CartOperationResult> UpdateQuantityAsync(string userId, int cartItemId, int newQuantity);
        Task<CartOperationResult> RemoveItemAsync(string userId, int cartItemId);
        Task<int> GetCartItemCountAsync(string userId);
        Task ReleaseExpiredCartItemsAsync(string? userId = null);
    }

    // Returned by every write operation so the controller can send
    // a clear JSON response back to the frontend JS.
    public class CartOperationResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int? NewQuantity { get; set; }       // updated quantity after the operation
        public int? NewStockQuantity { get; set; }  // remaining stock after the operation
        public int CartItemCount { get; set; }       // total items in cart after the operation

        public static CartOperationResult Ok(int newQuantity, int newStock, int cartCount) => new()
        {
            Success = true,
            NewQuantity = newQuantity,
            NewStockQuantity = newStock,
            CartItemCount = cartCount
        };

        public static CartOperationResult Fail(string message) => new()
        {
            Success = false,
            ErrorMessage = message
        };
    }

    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _db;

        // Cart items older than this are expired and stock is returned.
        private static readonly TimeSpan CartExpiry = TimeSpan.FromDays(3);

        // A medicine expiring within this many days blocks checkout.
        private static readonly int CriticalExpiryDays = 7;

        // A medicine expiring within this many days shows a soft warning.
        private static readonly int WarningExpiryDays = 30;

        public CartService(ApplicationDbContext db)
        {
            _db = db;
        }

        // =====================================================================
        // ReleaseExpiredCartItemsAsync
        // Called by the background service (all users) and as a lazy safety net
        // at the start of every cart operation (current user only).
        // =====================================================================
        public async Task ReleaseExpiredCartItemsAsync(string? userId = null)
        {
            var cutoff = DateTime.UtcNow - CartExpiry;

            // Build query — if userId is provided, only release that user's items.
            var expiredItems = await _db.CartItems
                .Include(ci => ci.Medicine)
                    .ThenInclude(m => m.Stock)
                .Where(ci => ci.UpdatedAt < cutoff &&
                             (userId == null || ci.UserId == userId))
                .ToListAsync();

            if (expiredItems.Count == 0)
                return;

            // Return each expired item's quantity back to stock.
            foreach (var item in expiredItems)
            {
                if (item.Medicine?.Stock != null)
                {
                    item.Medicine.Stock.Quantity += item.Quantity;
                    item.Medicine.Stock.UpdatedAt = DateTime.UtcNow;
                }
            }

            _db.CartItems.RemoveRange(expiredItems);
            await _db.SaveChangesAsync();
        }

        // =====================================================================
        // GetCartAsync
        // =====================================================================
        public async Task<List<CartItemViewModel>> GetCartAsync(string userId)
        {
            // Lazy safety net — release this user's expired items first.
            await ReleaseExpiredCartItemsAsync(userId);

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var items = await _db.CartItems
                .Include(ci => ci.Medicine)
                    .ThenInclude(m => m.Stock)
                .Include(ci => ci.Medicine)
                    .ThenInclude(m => m.ProductType)
                .Where(ci => ci.UserId == userId)
                .OrderBy(ci => ci.AddedAt)
                .ToListAsync();

            return items.Select(ci =>
            {
                var stock = ci.Medicine.Stock;
                var expiryDate = stock?.ExpiryDate ?? DateOnly.MaxValue;
                var daysToExpiry = expiryDate.DayNumber - today.DayNumber;

                return new CartItemViewModel
                {
                    Id = ci.Id,
                    MedicineId = ci.MedicineId,
                    Name = ci.Medicine.Name,
                    Description = BuildDescription(ci.Medicine),
                    IconType = ResolveIconType(ci.Medicine.ProductType?.Name),
                    ImageUrl = ci.Medicine.ImageUrl,
                    RequiresRx = ci.Medicine.RequiresPrescription,
                    UnitPrice = ci.Medicine.Price,
                    Quantity = ci.Quantity,
                    AvailableStock = stock?.Quantity ?? 0,
                    AddedAt = ci.AddedAt,
                    UpdatedAt = ci.UpdatedAt,
                    IsCriticalExpiry = daysToExpiry <= CriticalExpiryDays,
                    IsWarningExpiry = daysToExpiry <= WarningExpiryDays && daysToExpiry > CriticalExpiryDays
                };
            }).ToList();
        }

        // =====================================================================
        // AddToCartAsync
        // =====================================================================
        public async Task<CartOperationResult> AddToCartAsync(
            string userId, int medicineId, int quantity)
        {
            if (quantity <= 0)
                return CartOperationResult.Fail("Quantity must be at least 1.");

            // Lazy safety net.
            await ReleaseExpiredCartItemsAsync(userId);

            await using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var medicine = await _db.Medicines
                    .Include(m => m.Stock)
                    .FirstOrDefaultAsync(m => m.Id == medicineId);

                if (medicine == null)
                    return CartOperationResult.Fail("Medicine not found.");

                if (medicine.Stock == null)
                    return CartOperationResult.Fail("This medicine has no stock record.");

                // Block if medicine is in critical expiry tier.
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                var daysToExpiry = medicine.Stock.ExpiryDate.DayNumber - today.DayNumber;

                if (daysToExpiry <= CriticalExpiryDays)
                    return CartOperationResult.Fail(
                        "This medicine is expiring very soon and cannot be added to cart.");

                if (medicine.Stock.Quantity < quantity)
                    return CartOperationResult.Fail(
                        $"Only {medicine.Stock.Quantity} unit(s) available.");

                // Check if this medicine is already in the customer's cart.
                var existingItem = await _db.CartItems
                    .FirstOrDefaultAsync(ci =>
                        ci.UserId == userId && ci.MedicineId == medicineId);

                if (existingItem != null)
                {
                    // Already in cart — increase quantity.
                    existingItem.Quantity += quantity;
                    existingItem.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    // New cart item.
                    _db.CartItems.Add(new CartItem
                    {
                        UserId = userId,
                        MedicineId = medicineId,
                        Quantity = quantity,
                        AddedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }

                // Deduct from stock.
                medicine.Stock.Quantity -= quantity;
                medicine.Stock.UpdatedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                var cartCount = await GetCartItemCountAsync(userId);
                return CartOperationResult.Ok(
                    existingItem?.Quantity ?? quantity,
                    medicine.Stock.Quantity,
                    cartCount);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // =====================================================================
        // UpdateQuantityAsync
        // =====================================================================
        public async Task<CartOperationResult> UpdateQuantityAsync(
            string userId, int cartItemId, int newQuantity)
        {
            if (newQuantity <= 0)
                return CartOperationResult.Fail("Quantity must be at least 1.");

            // Lazy safety net.
            await ReleaseExpiredCartItemsAsync(userId);

            await using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var cartItem = await _db.CartItems
                    .Include(ci => ci.Medicine)
                        .ThenInclude(m => m.Stock)
                    .FirstOrDefaultAsync(ci =>
                        ci.Id == cartItemId && ci.UserId == userId);

                if (cartItem == null)
                    return CartOperationResult.Fail("Cart item not found.");

                if (cartItem.Medicine.Stock == null)
                    return CartOperationResult.Fail("Stock record missing.");

                var difference = newQuantity - cartItem.Quantity;

                if (difference > 0)
                {
                    // Customer wants more — check stock.
                    if (cartItem.Medicine.Stock.Quantity < difference)
                        return CartOperationResult.Fail(
                            $"Only {cartItem.Medicine.Stock.Quantity} more unit(s) available.");

                    cartItem.Medicine.Stock.Quantity -= difference;
                }
                else if (difference < 0)
                {
                    // Customer wants fewer — return the difference to stock.
                    cartItem.Medicine.Stock.Quantity += Math.Abs(difference);
                }

                cartItem.Quantity = newQuantity;
                cartItem.UpdatedAt = DateTime.UtcNow;
                cartItem.Medicine.Stock.UpdatedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                var cartCount = await GetCartItemCountAsync(userId);
                return CartOperationResult.Ok(
                    newQuantity,
                    cartItem.Medicine.Stock.Quantity,
                    cartCount);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // =====================================================================
        // RemoveItemAsync
        // =====================================================================
        public async Task<CartOperationResult> RemoveItemAsync(
            string userId, int cartItemId)
        {
            // Lazy safety net.
            await ReleaseExpiredCartItemsAsync(userId);

            await using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var cartItem = await _db.CartItems
                    .Include(ci => ci.Medicine)
                        .ThenInclude(m => m.Stock)
                    .FirstOrDefaultAsync(ci =>
                        ci.Id == cartItemId && ci.UserId == userId);

                if (cartItem == null)
                    return CartOperationResult.Fail("Cart item not found.");

                // Return quantity to stock.
                if (cartItem.Medicine.Stock != null)
                {
                    cartItem.Medicine.Stock.Quantity += cartItem.Quantity;
                    cartItem.Medicine.Stock.UpdatedAt = DateTime.UtcNow;
                }

                _db.CartItems.Remove(cartItem);

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                var cartCount = await GetCartItemCountAsync(userId);
                return CartOperationResult.Ok(0, cartItem.Medicine.Stock?.Quantity ?? 0, cartCount);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // =====================================================================
        // GetCartItemCountAsync
        // =====================================================================
        public async Task<int> GetCartItemCountAsync(string userId)
        {
            return await _db.CartItems
                .Where(ci => ci.UserId == userId)
                .SumAsync(ci => ci.Quantity);
        }

        // =====================================================================
        // Private helpers
        // =====================================================================

        private static string BuildDescription(Medicine medicine)
        {
            // Builds the subtitle line shown under the medicine name in the cart.
            // e.g. "Omeprazole 20mg · strip of 10"
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(medicine.GenericName))
                parts.Add(medicine.GenericName);

            if (!string.IsNullOrWhiteSpace(medicine.Unit))
                parts.Add(medicine.Unit);

            return string.Join(" · ", parts);
        }

        private static string ResolveIconType(string? productTypeName)
        {
            if (string.IsNullOrWhiteSpace(productTypeName))
                return "tablet";

            var name = productTypeName.ToLower();

            if (name.Contains("syrup") || name.Contains("suspension") ||
                name.Contains("drops") || name.Contains("injection"))
                return "bottle";

            return "tablet";
        }
    }
}
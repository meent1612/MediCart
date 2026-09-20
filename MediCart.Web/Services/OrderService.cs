using Microsoft.EntityFrameworkCore;
using MediCart.Web.Data;
using MediCart.Web.Models;

namespace MediCart.Web.Services
{
    public interface IOrderService
    {
        Task<OrderPlacementResult> PlaceOrderAsync(PlaceOrderRequest request);
        Task<OrderStatusChangeResult> RejectOrderAsync(int orderId, string reason, string adminId);
        Task<OrderStatusChangeResult> CancelOrderAsync(int orderId, string? reason, string adminId);
    }

    public class OrderPlacementResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int OrderId { get; set; }

        public static OrderPlacementResult Ok(int orderId) => new()
        {
            Success = true,
            OrderId = orderId
        };

        public static OrderPlacementResult Fail(string message) => new()
        {
            Success = false,
            ErrorMessage = message
        };
    }

    public class OrderStatusChangeResult
    {
        public bool Success { get; set; }
        public bool NotFound { get; set; }
        public string? ErrorMessage { get; set; }

        public static OrderStatusChangeResult Ok() => new() { Success = true };

        public static OrderStatusChangeResult Missing() => new()
        {
            Success = false,
            NotFound = true,
            ErrorMessage = "Order not found."
        };

        public static OrderStatusChangeResult Fail(string message) => new()
        {
            Success = false,
            ErrorMessage = message
        };
    }

    public class PlaceOrderRequest
    {
        public string UserId { get; set; } = string.Empty;
        public int DivisionId { get; set; }
        public int CityId { get; set; }
        public string AddressLine { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string? PrescriptionImageUrl { get; set; }   // null if no Rx items in cart
    }

    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _db;

        public OrderService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<OrderPlacementResult> PlaceOrderAsync(PlaceOrderRequest request)
        {
            // Load cart items with medicine + stock
            var cartItems = await _db.CartItems
                .Include(ci => ci.Medicine)
                .Where(ci => ci.UserId == request.UserId)
                .ToListAsync();

            if (cartItems.Count == 0)
                return OrderPlacementResult.Fail("Your cart is empty.");

            // Server-side enforcement: if any medicine in the cart requires a
            // prescription, a prescription image must have been uploaded.
            bool requiresPrescription = cartItems.Any(ci => ci.Medicine.RequiresPrescription);
            if (requiresPrescription && string.IsNullOrWhiteSpace(request.PrescriptionImageUrl))
            {
                return OrderPlacementResult.Fail(
                    "This order contains a medicine that requires a prescription. Please attach one before placing the order.");
            }

            // Load division for delivery charge
            var division = await _db.Divisions
                .FirstOrDefaultAsync(d => d.Id == request.DivisionId);

            if (division == null)
                return OrderPlacementResult.Fail("Invalid division.");

            // Calculate totals
            decimal subtotal = cartItems.Sum(ci => ci.Medicine.Price * ci.Quantity);
            decimal deliveryCharge = division.DeliveryCharge;
            decimal totalAmount = subtotal + deliveryCharge;

            // Check sensitivity flagging thresholds — logic lives in
            // SensitivityFlagHelper so display-time recomputation
            // (Flagged Orders page) always matches this check exactly.
            bool isFlagged = cartItems.Any(ci =>
                SensitivityFlagHelper.IsOverThreshold(ci.Medicine.SensitivityLevel, ci.Quantity));

            await using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                // Create the order
                var order = new Order
                {
                    UserId = request.UserId,
                    DivisionId = request.DivisionId,
                    CityId = request.CityId,
                    AddressLine = request.AddressLine,
                    Phone = request.Phone,
                    DeliveryCharge = deliveryCharge,
                    TotalAmount = totalAmount,
                    PaymentMethod = request.PaymentMethod,
                    Status = "Pending",
                    IsFlagged = isFlagged,
                    CreatedAt = DateTime.UtcNow
                };

                _db.Orders.Add(order);
                await _db.SaveChangesAsync();  // gets order.Id

                // Create order items
                var orderItems = cartItems.Select(ci => new OrderItem
                {
                    OrderId = order.Id,
                    MedicineId = ci.MedicineId,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.Medicine.Price
                }).ToList();

                _db.OrderItems.AddRange(orderItems);

                // Create prescription record if needed
                if (!string.IsNullOrWhiteSpace(request.PrescriptionImageUrl))
                {
                    _db.Prescriptions.Add(new Prescription
                    {
                        UserId = request.UserId,
                        OrderId = order.Id,
                        ImageUrl = request.PrescriptionImageUrl,
                        Status = "pending",
                        UploadedAt = DateTime.UtcNow
                    });
                }

                // Create payment record
                _db.Payments.Add(new Payment
                {
                    OrderId = order.Id,
                    UserId = request.UserId,
                    Amount = totalAmount,
                    Method = request.PaymentMethod,
                    Status = request.PaymentMethod == "Cash on delivery" ? "pending" : "completed",
                    PaidAt = request.PaymentMethod == "Cash on delivery" ? null : DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                });

                // Clear the cart. Stock was already deducted when each item was
                // added to the cart, so the order now owns that reservation.
                _db.CartItems.RemoveRange(cartItems);

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                return OrderPlacementResult.Ok(order.Id);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // =====================================================================
        // Reject (Pending only) and Cancel (Processing / Shipped only).
        // Both end the order, so both give the reserved stock back.
        // =====================================================================

        public Task<OrderStatusChangeResult> RejectOrderAsync(int orderId, string reason, string adminId)
        {
            return CloseOrderAsync(
                orderId,
                newStatus: "Rejected",
                allowedFrom: new[] { "Pending" },
                reason: reason.Trim(),
                adminId: adminId,
                auditType: AuditActionTypes.Rejected);
        }

        public Task<OrderStatusChangeResult> CancelOrderAsync(int orderId, string? reason, string adminId)
        {
            return CloseOrderAsync(
                orderId,
                newStatus: "Cancelled",
                allowedFrom: new[] { "Processing", "Shipped" },
                reason: string.IsNullOrWhiteSpace(reason) ? null : reason.Trim(),
                adminId: adminId,
                auditType: AuditActionTypes.Cancelled);
        }

        private async Task<OrderStatusChangeResult> CloseOrderAsync(
            int orderId,
            string newStatus,
            string[] allowedFrom,
            string? reason,
            string adminId,
            string auditType)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                // 1. Change the status only if the order is still in an allowed state.
                //    This is one atomic UPDATE, so if two admins click at the same time
                //    only one of them gets rowsChanged == 1 and restores stock.
                int rowsChanged = await _db.Orders
                    .Where(o => o.Id == orderId && allowedFrom.Contains(o.Status))
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(o => o.Status, newStatus)
                        .SetProperty(o => o.RejectionReason, reason));

                if (rowsChanged == 0)
                {
                    var currentStatus = await _db.Orders
                        .AsNoTracking()
                        .Where(o => o.Id == orderId)
                        .Select(o => o.Status)
                        .FirstOrDefaultAsync();

                    await transaction.RollbackAsync();

                    if (currentStatus == null)
                        return OrderStatusChangeResult.Missing();

                    string verb = newStatus == "Rejected" ? "reject" : "cancel";
                    string message = $"Cannot {verb} — order is '{currentStatus}'.";

                    if (newStatus == "Cancelled" && currentStatus == "Pending")
                        message += " A Pending order must be rejected instead.";

                    return OrderStatusChangeResult.Fail(message);
                }

                // 2. Give the reserved stock back (atomic add, no read-then-write).
                var items = await _db.OrderItems
                    .Where(oi => oi.OrderId == orderId)
                    .Select(oi => new { oi.MedicineId, oi.Quantity })
                    .ToListAsync();

                var now = DateTime.UtcNow;

                foreach (var item in items)
                {
                    await _db.Stocks
                        .Where(s => s.MedicineId == item.MedicineId)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(x => x.Quantity, x => x.Quantity + item.Quantity)
                            .SetProperty(x => x.UpdatedAt, now));
                }

                // 3. Audit log: only the admin's Reject/Cancel action, not the stock restore.
                _db.AuditLogs.Add(new AuditLog
                {
                    AdminId = adminId,
                    Action = $"{newStatus} order MC-{10000 + orderId}",
                    ActionType = auditType,
                    TableName = "Orders",
                    RecordId = orderId,
                    CreatedAt = now
                });

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                return OrderStatusChangeResult.Ok();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
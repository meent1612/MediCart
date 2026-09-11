using Microsoft.EntityFrameworkCore;
using MediCart.Web.Data;
using MediCart.Web.Models;

namespace MediCart.Web.Services
{
    public interface IOrderService
    {
        Task<OrderPlacementResult> PlaceOrderAsync(PlaceOrderRequest request);
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

                // Clear the cart
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
    }
}
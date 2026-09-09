using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MediCart.Web.Data;
using MediCart.Web.Models;

namespace MediCart.Web.Controllers
{
    [Authorize(Roles = "Customer")]
    public class ConfirmationController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public ConfirmationController(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // GET /Confirmation/{id}
        public async Task<IActionResult> Index(int id)
        {
            var userId = _userManager.GetUserId(User)!;

            var order = await _db.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Medicine)
                .Include(o => o.Division)
                .Include(o => o.City)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            // Order not found or belongs to a different customer.
            if (order == null)
                return RedirectToAction("Index", "Cart");

            var model = BuildViewModel(order);
            return View(model);
        }

        // Shared helper — builds the ViewModel from a real Order entity.
        // OrdersController reuses the same shape.
        public static OrderConfirmationViewModel BuildViewModel(Order order)
        {
            var stages = new List<OrderTrackingStage>
            {
                new() { Label = "Pending",    IsDone = true,
                        Timestamp = order.CreatedAt.ToLocalTime().ToString("dd MMM, h:mm tt") },
                new() { Label = "Processing", IsDone = order.Status is "Processing" or "Shipped" or "Delivered",
                        Timestamp = order.Status is "Processing" or "Shipped" or "Delivered" ? "In progress" : "Awaiting" },
                new() { Label = "Shipped",    IsDone = order.Status is "Shipped" or "Delivered",
                        Timestamp = order.Status is "Shipped" or "Delivered" ? "On the way" : "Awaiting" },
                new() { Label = "Delivered",  IsDone = order.Status == "Delivered",
                        Timestamp = order.Status == "Delivered" ? "Completed" : "Awaiting" }
            };

            // If rejected, replace the last stage.
            if (order.Status == "Rejected")
            {
                stages[3] = new OrderTrackingStage
                {
                    Label = "Rejected",
                    IsDone = true,
                    Timestamp = "Order rejected"
                };
            }

            return new OrderConfirmationViewModel
            {
                RealOrderId = order.Id,
                OrderNumber = "MC-" + (10000 + order.Id),
                PlacedAt = order.CreatedAt.ToLocalTime().ToString("dd MMM yyyy, h:mm tt"),
                StatusLabel = order.Status,
                Stages = stages,
                CustomerName = order.User.FullName,
                DeliveryAddress = $"{order.AddressLine}, {order.City.Name}, {order.Division.Name}",
                Phone = order.Phone,
                AdminNote = order.RejectionReason,
                PaymentMethod = order.PaymentMethod,
                Items = order.OrderItems.Select(oi => new CheckoutLineItemViewModel
                {
                    Id = oi.MedicineId,
                    Name = oi.Medicine.Name,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                }).ToList(),
                DeliveryCharge = order.DeliveryCharge
            };
        }
    }
}
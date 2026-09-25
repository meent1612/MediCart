using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MediCart.Web.Data;
using MediCart.Web.Models;
using MediCart.Web.Services;
using Microsoft.AspNetCore.Authorization;

namespace MediCart.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminOrdersController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOrderService _orderService;

        public AdminOrdersController(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            IOrderService orderService)
        {
            _db = db;
            _userManager = userManager;
            _orderService = orderService;
        }

        // =====================
        // Incoming Orders (list)
        // =====================

        [HttpGet]
        public async Task<IActionResult> IncomingOrders(string? status)
        {
            var query = _db.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .Include(o => o.Prescription)
                .AsQueryable();

            if (string.IsNullOrWhiteSpace(status) || status == "Active")
            {
                query = query.Where(o => o.Status != "Delivered" && o.Status != "Rejected" && o.Status != "Cancelled");
                status = "Active";
            }
            else if (status != "All")
            {
                query = query.Where(o => o.Status == status);
            }

            var orders = await query
                .OrderBy(o => o.CreatedAt)
                .Select(o => new AdminOrderRowViewModel
                {
                    Id = o.Id,
                    OrderNumber = "MC-" + (10000 + o.Id),
                    CustomerName = o.User.FullName,
                    ItemCount = o.OrderItems.Sum(oi => oi.Quantity),
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    IsFlagged = o.IsFlagged,
                    HasPrescription = o.Prescription != null,
                    CreatedAt = o.CreatedAt
                })
                .ToListAsync();

            var model = new AdminOrderListViewModel
            {
                Orders = orders,
                StatusFilter = status,
                TotalCount = orders.Count
            };

            return View(model);
        }

        // =====================
        // Order Detail
        // =====================

        [HttpGet]
        public async Task<IActionResult> OrderDetail(int id)
        {
            var order = await _db.Orders
                .Include(o => o.User)
                .Include(o => o.Division)
                .Include(o => o.City)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Medicine)
                .Include(o => o.Prescription)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                TempData["OrderError"] = "Order not found.";
                return RedirectToAction(nameof(IncomingOrders));
            }

            var model = BuildDetailViewModel(order);
            return View(model);
        }

        // =====================
        // Approve / Reject / Ship / Deliver / Cancel
        // =====================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var order = await _db.Orders.FindAsync(id);

            if (order == null)
            {
                TempData["OrderError"] = "Order not found.";
                return RedirectToAction(nameof(IncomingOrders));
            }

            if (order.Status != "Pending")
            {
                TempData["OrderError"] = $"Cannot approve — order is already '{order.Status}'.";
                return RedirectToAction(nameof(OrderDetail), new { id });
            }

            order.Status = "Processing";

            var adminId = _userManager.GetUserId(User);
            if (adminId != null)
                LogAction(adminId, $"Approved order MC-{10000 + order.Id}", order.Id, AuditActionTypes.Approved);

            await _db.SaveChangesAsync();

            TempData["OrderSuccess"] = $"Order MC-{10000 + order.Id} approved and moved to Processing.";
            return RedirectToAction(nameof(OrderDetail), new { id });
        }

        // Reject: Pending orders only. Stock and Payment are restored/closed inside OrderService.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["OrderError"] = "Enter a reason for rejecting this order.";
                return RedirectToAction(nameof(OrderDetail), new { id });
            }

            var adminId = _userManager.GetUserId(User);
            if (adminId == null)
                return Challenge();

            var result = await _orderService.RejectOrderAsync(id, reason, adminId);

            if (!result.Success)
            {
                TempData["OrderError"] = result.ErrorMessage;

                if (result.NotFound)
                    return RedirectToAction(nameof(IncomingOrders));

                return RedirectToAction(nameof(OrderDetail), new { id });
            }

            TempData["OrderSuccess"] = $"Order MC-{10000 + id} rejected.";
            return RedirectToAction(nameof(OrderDetail), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkShipped(int id)
        {
            var order = await _db.Orders.FindAsync(id);

            if (order == null)
            {
                TempData["OrderError"] = "Order not found.";
                return RedirectToAction(nameof(IncomingOrders));
            }

            if (order.Status != "Processing")
            {
                TempData["OrderError"] = $"Cannot mark shipped — order is '{order.Status}', not 'Processing'.";
                return RedirectToAction(nameof(OrderDetail), new { id });
            }

            order.Status = "Shipped";

            var adminId = _userManager.GetUserId(User);
            if (adminId != null)
                LogAction(adminId, $"Marked order MC-{10000 + order.Id} as Shipped", order.Id, AuditActionTypes.MarkedShipped);

            await _db.SaveChangesAsync();

            TempData["OrderSuccess"] = $"Order MC-{10000 + order.Id} marked as Shipped.";
            return RedirectToAction(nameof(OrderDetail), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkDelivered(int id)
        {
            var order = await _db.Orders.FindAsync(id);

            if (order == null)
            {
                TempData["OrderError"] = "Order not found.";
                return RedirectToAction(nameof(IncomingOrders));
            }

            if (order.Status != "Shipped")
            {
                TempData["OrderError"] = $"Cannot mark delivered — order is '{order.Status}', not 'Shipped'.";
                return RedirectToAction(nameof(OrderDetail), new { id });
            }

            order.Status = "Delivered";

            // Cash on delivery is only ever collected at the door — mark the
            // Payment as completed now that delivery has happened. bKash/Card
            // payments were already completed at order placement (see
            // OrderService.PlaceOrderAsync) and are left untouched here.
            //
            // Queried directly by OrderId rather than via order.Payments:
            // that navigation is backed by an unused shadow FK
            // (Payment.OrderId1, never set anywhere) and is always empty.
            // Same direct-query pattern as OrderService.CloseOrderAsync.
            var payment = await _db.Payments.FirstOrDefaultAsync(p => p.OrderId == id);
            if (payment != null && payment.Method == PaymentMethods.CashOnDelivery)
            {
                payment.Status = PaymentStatuses.Completed;
                payment.PaidAt = DateTime.UtcNow;
            }

            var adminId = _userManager.GetUserId(User);
            if (adminId != null)
                LogAction(adminId, $"Marked order MC-{10000 + order.Id} as Delivered", order.Id, AuditActionTypes.MarkedDelivered);

            await _db.SaveChangesAsync();

            TempData["OrderSuccess"] = $"Order MC-{10000 + order.Id} marked as Delivered.";
            return RedirectToAction(nameof(OrderDetail), new { id });
        }

        // Cancel: Processing or Shipped orders only. A reason is required.
        // Status change, stock restore, Payment close-out, and audit log are
        // all handled inside OrderService.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int id, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["OrderError"] = "Enter a reason for cancelling this order.";
                return RedirectToAction(nameof(OrderDetail), new { id });
            }

            var adminId = _userManager.GetUserId(User);
            if (adminId == null)
                return Challenge();

            var result = await _orderService.CancelOrderAsync(id, reason, adminId);

            if (!result.Success)
            {
                TempData["OrderError"] = result.ErrorMessage;
                return RedirectToAction(nameof(OrderDetail), new { id });
            }

            TempData["OrderSuccess"] = $"Order MC-{10000 + id} has been cancelled.";
            return RedirectToAction(nameof(OrderDetail), new { id });
        }

        // =====================
        // Flagged Orders
        // =====================

                       [HttpGet]
        public async Task<IActionResult> FlaggedOrders(string? tier)
        {
            tier ??= "All";

            // Flagged Orders is a "needs initial review" queue only — it
            // shows a flagged order for as long as no admin action has
            // been taken on it (Status == Pending). The moment it's
            // Approved (-> Processing) or Rejected (-> Rejected), it drops
            // off this list for good, even though IsFlagged stays true on
            // the row forever. A later Cancel (only possible after
            // Approve, from Processing/Shipped) never needs handling here
            // separately, since the order already left this list at the
            // Approve step. Full status history for a flagged order,
            // including Cancelled, is still visible in IncomingOrders via
            // its own status dropdown (Active/All/Pending/Processing/
            // Shipped/Delivered/Rejected/Cancelled).
            var orders = await _db.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Medicine)
                .Where(o => o.IsFlagged && o.Status == "Pending")
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var rows = orders
                .Select(o => new AdminFlaggedOrderRowViewModel
                {
                    Id = o.Id,
                    OrderNumber = "MC-" + (10000 + o.Id),
                    CustomerName = o.User.FullName,
                    Status = o.Status,
                    CreatedAt = o.CreatedAt,
                    FlaggedItems = ComputeFlaggedItems(o.OrderItems)
                })
                // Only keep orders that have at least one flagged item at the
                // selected tier — an order can carry flags at more than one
                // tier, so this is "any match", not "every item matches".
                .Where(row => tier == "All" ||
                    row.FlaggedItems.Any(fi =>
                        string.Equals(fi.SensitivityLevel, tier, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            var model = new AdminFlaggedOrdersListViewModel
            {
                Orders = rows,
                TotalCount = rows.Count,
                TierFilter = tier
            };

            return View(model);
        }

        // =====================
        // Helpers
        // =====================

        private static AdminOrderDetailViewModel BuildDetailViewModel(Order order)
        {
            return new AdminOrderDetailViewModel
            {
                Id = order.Id,
                OrderNumber = "MC-" + (10000 + order.Id),
                Status = order.Status,
                IsFlagged = order.IsFlagged,
                CreatedAt = order.CreatedAt,
                CustomerName = order.User.FullName,
                CustomerEmail = order.User.Email ?? "",
                Phone = order.Phone,
                DeliveryAddress = $"{order.AddressLine}, {order.City.Name}, {order.Division.Name}",
                PaymentMethod = order.PaymentMethod,
                DeliveryCharge = order.DeliveryCharge,
                RejectionReason = order.RejectionReason,
                Items = order.OrderItems.Select(oi => new AdminOrderItemRowViewModel
                {
                    MedicineName = oi.Medicine.Name,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                }).ToList(),
                FlaggedItems = order.IsFlagged
                    ? ComputeFlaggedItems(order.OrderItems)
                    : new List<FlaggedItemViewModel>(),
                PrescriptionImageUrl = order.Prescription?.ImageUrl,
                PrescriptionStatus = order.Prescription?.Status
            };
        }

        // Recomputes every medicine+tier combination in this order that
        // crosses its sensitivity threshold, using the same rule OrderService
        // applied at placement time (see SensitivityFlagHelper).
        private static List<FlaggedItemViewModel> ComputeFlaggedItems(ICollection<OrderItem> items)
        {
            var result = new List<FlaggedItemViewModel>();

            foreach (var oi in items)
            {
                var level = oi.Medicine.SensitivityLevel;

                if (SensitivityFlagHelper.IsOverThreshold(level, oi.Quantity))
                {
                    result.Add(new FlaggedItemViewModel
                    {
                        MedicineName = oi.Medicine.Name,
                        SensitivityLevel = level!.ToLower(),
                        Quantity = oi.Quantity,
                        Threshold = SensitivityFlagHelper.GetThreshold(level)!.Value
                    });
                }
            }

            return result;
        }

        private void LogAction(string adminId, string action, int orderId, string actionType)
        {
            _db.AuditLogs.Add(new AuditLog
            {
                AdminId = adminId,
                Action = action,
                ActionType = actionType,
                TableName = "Orders",
                RecordId = orderId,
                CreatedAt = DateTime.UtcNow
            });
        }
    }
}
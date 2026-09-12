using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MediCart.Web.Data;
using MediCart.Web.Models;
using MediCart.Web.Services;
// using Microsoft.AspNetCore.Authorization; // enable once every teammate has tested login with the Admin role

namespace MediCart.Web.Controllers
{
    // [Authorize(Roles = "Admin")] // TODO: turn this on before demo — matches AdminController's current state
    public class AdminOrdersController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminOrdersController(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
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
                query = query.Where(o => o.Status != "Delivered" && o.Status != "Rejected");
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
        // Approve / Reject / Ship / Deliver
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
                LogAction(adminId, $"Approved order MC-{10000 + order.Id}", order.Id);

            await _db.SaveChangesAsync();

            TempData["OrderSuccess"] = $"Order MC-{10000 + order.Id} approved and moved to Processing.";
            return RedirectToAction(nameof(OrderDetail), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["OrderError"] = "Enter a reason for rejecting this order.";
                return RedirectToAction(nameof(OrderDetail), new { id });
            }

            var order = await _db.Orders.FindAsync(id);

            if (order == null)
            {
                TempData["OrderError"] = "Order not found.";
                return RedirectToAction(nameof(IncomingOrders));
            }

            if (order.Status != "Pending")
            {
                TempData["OrderError"] = $"Cannot reject — order is already '{order.Status}'.";
                return RedirectToAction(nameof(OrderDetail), new { id });
            }

            order.Status = "Rejected";
            order.RejectionReason = reason.Trim();

            var adminId = _userManager.GetUserId(User);
            if (adminId != null)
                LogAction(adminId, $"Rejected order MC-{10000 + order.Id}", order.Id);

            await _db.SaveChangesAsync();

            TempData["OrderSuccess"] = $"Order MC-{10000 + order.Id} rejected.";
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
                LogAction(adminId, $"Marked order MC-{10000 + order.Id} as Shipped", order.Id);

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

            var adminId = _userManager.GetUserId(User);
            if (adminId != null)
                LogAction(adminId, $"Marked order MC-{10000 + order.Id} as Delivered", order.Id);

            await _db.SaveChangesAsync();

            TempData["OrderSuccess"] = $"Order MC-{10000 + order.Id} marked as Delivered.";
            return RedirectToAction(nameof(OrderDetail), new { id });
        }

        // =====================
        // Flagged Orders
        // =====================

               [HttpGet]
        public async Task<IActionResult> FlaggedOrders(string? tier)
        {
            tier ??= "All";

            var orders = await _db.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Medicine)
                .Where(o => o.IsFlagged)
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

        private void LogAction(string adminId, string action, int orderId)
        {
            _db.AuditLogs.Add(new AuditLog
            {
                AdminId = adminId,
                Action = action,
                TableName = "Orders",
                RecordId = orderId,
                CreatedAt = DateTime.UtcNow
            });
        }
    }
}
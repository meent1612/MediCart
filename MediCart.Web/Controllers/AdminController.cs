using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MediCart.Web.Data;
using MediCart.Web.Models;
using MediCart.Web.Services;
using Microsoft.AspNetCore.Authorization;

namespace MediCart.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly Services.IImageUploadService _imageUploadService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(
            ApplicationDbContext db,
            Services.IImageUploadService imageUploadService,
            UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _imageUploadService = imageUploadService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var currentAdmin = await _userManager.GetUserAsync(User);

            if (currentAdmin == null)
            {
                return View(new AdminProfileViewModel());
            }

            var roles = await _userManager.GetRolesAsync(currentAdmin);

            var nameParts = currentAdmin.FullName
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var initials = nameParts.Length >= 2
                ? $"{nameParts[0][0]}{nameParts[1][0]}".ToUpperInvariant()
                : currentAdmin.FullName.Length >= 2
                    ? currentAdmin.FullName.Substring(0, 2).ToUpperInvariant()
                    : currentAdmin.FullName.ToUpperInvariant();

            var recentActivity = await _db.AuditLogs
                .Where(a => a.AdminId == currentAdmin.Id)
                .OrderByDescending(a => a.CreatedAt)
                .Take(8)
                .Select(a => new AdminActivityItemViewModel
                {
                    Action = a.Action,
                    TableName = a.TableName,
                    RecordId = a.RecordId,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();

            var model = new AdminProfileViewModel
            {
                FullName = currentAdmin.FullName,
                Email = currentAdmin.Email ?? "",
                PhoneNumber = currentAdmin.PhoneNumber,
                Initials = initials,
                RoleDisplay = roles.FirstOrDefault() ?? "Admin",
                RecentActivity = recentActivity
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(AdminProfileFormViewModel form)
        {
            if (!ModelState.IsValid)
            {
                TempData["ProfileError"] = "Please enter a valid name and phone number.";
                return RedirectToAction(nameof(Profile));
            }

            var currentAdmin = await _userManager.GetUserAsync(User);
            if (currentAdmin == null)
            {
                TempData["ProfileError"] = "You need to be logged in to update your profile.";
                return RedirectToAction(nameof(Profile));
            }

            currentAdmin.FullName = form.FullName.Trim();
            currentAdmin.PhoneNumber = string.IsNullOrWhiteSpace(form.PhoneNumber)
                ? null
                : form.PhoneNumber.Trim();

            var result = await _userManager.UpdateAsync(currentAdmin);

            TempData["ProfileSuccess"] = result.Succeeded
                ? "Profile updated."
                : null;

            if (!result.Succeeded)
            {
                TempData["ProfileError"] = string.Join(" ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction(nameof(Profile));
        }

        [HttpGet]
        public async Task<IActionResult> Medicines(
            string? search,
            int? categoryId,
            int? subCategoryId,
            int? productTypeId)
        {
            var query = _db.Medicines
                .Include(m => m.Category)
                .Include(m => m.SubCategory)
                .Include(m => m.ProductType)
                .Include(m => m.Stock)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(m =>
                    EF.Functions.ILike(m.Name, $"%{term}%"));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(m =>
                    m.CategoryId == categoryId.Value);
            }

            if (subCategoryId.HasValue)
            {
                query = query.Where(m =>
                    m.SubCategoryId == subCategoryId.Value);
            }

            if (productTypeId.HasValue)
            {
                query = query.Where(m =>
                    m.ProductTypeId == productTypeId.Value);
            }

            var medicines = await query
                .OrderBy(m => m.Name)
                .Select(m => new MedicineListRowViewModel
                {
                    Id = m.Id,
                    Name = m.Name,
                    CategoryName = m.Category.Name,
                    SubCategoryName = m.SubCategory != null ? m.SubCategory.Name : null,
                    ProductTypeName = m.ProductType.Name,
                    Manufacturer = m.Manufacturer,
                    Price = m.Price,
                    StockQuantity = m.Stock != null
                        ? m.Stock.Quantity
                        : 0,
                    ExpiryDate = m.Stock != null
                        ? m.Stock.ExpiryDate
                        : DateOnly.MinValue,
                    SensitivityLevel = m.SensitivityLevel,
                    RequiresPrescription = m.RequiresPrescription
                })
                .ToListAsync();

            var model = new AdminMedicinesPageViewModel
            {
                Medicines = medicines,
                CategoryOptions =
                    await BuildCategoryDropdownOptionsAsync(),
                ProductTypeOptions =
                    await BuildProductTypeDropdownOptionsAsync(),
                Search = search,
                CategoryId = categoryId,
                SubCategoryId = subCategoryId,
                ProductTypeId = productTypeId,
                TotalCount = medicines.Count
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var todayUtc = DateTime.UtcNow.Date;
            var sevenDaysAgo = todayUtc.AddDays(-6);
            var todayDateOnly = DateOnly.FromDateTime(DateTime.UtcNow);
            var expiryThreshold = todayDateOnly.AddDays(StockExpiryHelper.WarningExpiryDays);

            var totalOrders = await _db.Orders.CountAsync();
            var totalRevenue = await _db.Orders.SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;
            var pendingProcessing = await _db.Orders.CountAsync(o => o.Status == "Pending" || o.Status == "Processing");
            var flaggedCount = await _db.Orders.CountAsync(o => o.IsFlagged);
            var lowStockCount = await _db.Stocks.CountAsync(s => s.Quantity <= StockExpiryHelper.LowStockThreshold);

            var ordersLast7Days = await _db.Orders
                .Where(o => o.CreatedAt >= sevenDaysAgo)
                .Select(o => new { o.CreatedAt })
                .ToListAsync();

            var chartDays = new List<string>();
            var chartCounts = new List<int>();

            for (int i = 6; i >= 0; i--)
            {
                var targetDay = todayUtc.AddDays(-i);
                var label = targetDay.ToString("dd MMM");
                var count = ordersLast7Days.Count(o => o.CreatedAt.Date == targetDay);
                chartDays.Add(label);
                chartCounts.Add(count);
            }

            var recentOrders = await _db.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.CreatedAt)
                .Take(7)
                .Select(o => new DashboardRecentOrderViewModel
                {
                    Id = o.Id,
                    OrderNumber = "MC-" + (10000 + o.Id),
                    CustomerName = o.User != null && !string.IsNullOrWhiteSpace(o.User.FullName) ? o.User.FullName : (o.User != null ? o.User.UserName! : "Customer"),
                    ItemCount = o.OrderItems.Sum(i => i.Quantity),
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    CreatedAt = o.CreatedAt
                })
                .ToListAsync();

            var attentionItems = new List<DashboardAttentionItemViewModel>();

            var flaggedOrdersQuery = _db.Orders
                .Include(o => o.User)
                .Where(o => o.IsFlagged && o.Status != "Delivered" && o.Status != "Rejected")
                .OrderByDescending(o => o.CreatedAt);

            var totalFlaggedCount = await flaggedOrdersQuery.CountAsync();
            var flaggedOrders = await flaggedOrdersQuery.Take(4).ToListAsync();

            var expiringQuery = _db.Stocks
                .Include(s => s.Medicine)
                .Where(s => s.ExpiryDate <= expiryThreshold)
                .OrderBy(s => s.ExpiryDate);

            var totalExpiringCount = await expiringQuery.CountAsync();
            var expiringItems = await expiringQuery.Take(2).ToListAsync();

            var lowStockQuery = _db.Stocks
                .Include(s => s.Medicine)
                .Where(s => s.Quantity < 10)
                .OrderBy(s => s.Quantity);

            var totalLowStockCount = await lowStockQuery.CountAsync();
            var lowStockItems = await lowStockQuery.Take(2).ToListAsync();

            var totalAttentionCount = totalFlaggedCount + totalExpiringCount + totalLowStockCount;

            // 1. Priority 1: Flagged orders
            foreach (var fo in flaggedOrders)
            {
                if (attentionItems.Count >= 4) break;
                attentionItems.Add(new DashboardAttentionItemViewModel
                {
                    Type = "FlaggedOrder",
                    Title = $"Order #MC-{10000 + fo.Id} flagged",
                    Subtitle = $"{fo.User?.FullName ?? "Customer"} · ৳{fo.TotalAmount:0} · Status: {fo.Status}",
                    Severity = "danger",
                    ActionUrl = Url.Action("OrderDetail", "AdminOrders", new { id = fo.Id }) ?? $"/AdminOrders/OrderDetail/{fo.Id}",
                    ActionText = "Review order"
                });
            }

            // 2. Priority 2: Soonest-to-expire (up to 2, capped at 4 total)
            var handledMedicineIds = new HashSet<int>();
            foreach (var st in expiringItems)
            {
                if (attentionItems.Count >= 4) break;
                handledMedicineIds.Add(st.MedicineId);

                var daysUntil = StockExpiryHelper.DaysUntilExpiry(st.ExpiryDate);
                string sub;
                string sev;

                if (StockExpiryHelper.IsExpired(daysUntil))
                {
                    sub = $"Expired {Math.Abs(daysUntil)} day(s) ago";
                    sev = "danger";
                }
                else if (StockExpiryHelper.IsCriticalExpiry(daysUntil))
                {
                    sub = daysUntil == 0 ? "Expires today" : $"Expires in {daysUntil} day(s) — Critical";
                    sev = "danger";
                }
                else
                {
                    sub = $"Expires in {daysUntil} day(s) — Warning";
                    sev = "warning";
                }

                attentionItems.Add(new DashboardAttentionItemViewModel
                {
                    Type = "ExpiringSoon",
                    Title = st.Medicine?.Name ?? "Medicine",
                    Subtitle = sub,
                    Severity = sev,
                    ActionUrl = Url.Action("StockExpiry", "Admin") ?? "/Admin/StockExpiry",
                    ActionText = "View"
                });
            }

            // 3. Priority 3: Lowest stock (up to 2, capped at 4 total)
            foreach (var st in lowStockItems)
            {
                if (attentionItems.Count >= 4) break;
                if (handledMedicineIds.Contains(st.MedicineId)) continue;

                var isOut = StockExpiryHelper.IsOutOfStock(st.Quantity);
                string sub = isOut
                    ? "Out of stock (0 units left)"
                    : $"{st.Quantity} {(st.Quantity == 1 ? "unit" : "units")} left — Low stock";
                string sev = isOut ? "danger" : "warning";

                attentionItems.Add(new DashboardAttentionItemViewModel
                {
                    Type = "LowStock",
                    Title = st.Medicine?.Name ?? "Medicine",
                    Subtitle = sub,
                    Severity = sev,
                    ActionUrl = Url.Action("EditMedicine", "Admin", new { id = st.MedicineId }) ?? $"/Admin/EditMedicine/{st.MedicineId}",
                    ActionText = "Restock"
                });
            }

            var avgOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0m;

            // Best Selling Medicines (Top 5 by units sold)
            var bestSellingGroup = await _db.OrderItems
                .Include(oi => oi.Medicine)
                .GroupBy(oi => new { oi.MedicineId, oi.Medicine.Name })
                .Select(g => new
                {
                    MedicineId = g.Key.MedicineId,
                    MedicineName = g.Key.Name,
                    UnitsSold = g.Sum(oi => oi.Quantity),
                    Revenue = g.Sum(oi => oi.Quantity * oi.UnitPrice)
                })
                .OrderByDescending(x => x.UnitsSold)
                .ThenByDescending(x => x.Revenue)
                .Take(5)
                .ToListAsync();

            var bestSellingList = bestSellingGroup.Select((item, idx) => new DashboardBestSellingMedicineViewModel
            {
                Rank = idx + 1,
                MedicineId = item.MedicineId,
                MedicineName = item.MedicineName,
                UnitsSold = item.UnitsSold,
                Revenue = item.Revenue
            }).ToList();

            // Top Categories breakdown (by revenue share)
            var categoryRevenueGroup = await _db.OrderItems
                .Include(oi => oi.Medicine)
                    .ThenInclude(m => m.Category)
                .Where(oi => oi.Medicine != null && oi.Medicine.Category != null)
                .GroupBy(oi => oi.Medicine.Category.Name)
                .Select(g => new
                {
                    CategoryName = g.Key,
                    Revenue = g.Sum(oi => oi.Quantity * oi.UnitPrice)
                })
                .OrderByDescending(x => x.Revenue)
                .Take(4)
                .ToListAsync();

            var totalCategoryRevenue = categoryRevenueGroup.Sum(x => x.Revenue);
            var topCategoriesList = categoryRevenueGroup.Select(x => new DashboardTopCategoryViewModel
            {
                CategoryName = x.CategoryName,
                Revenue = x.Revenue,
                Percentage = totalCategoryRevenue > 0 ? Math.Round((x.Revenue / totalCategoryRevenue) * 100m, 1) : 0m
            }).ToList();

            var vm = new AdminDashboardViewModel
            {
                TotalRevenue = totalRevenue,
                AverageOrderValue = avgOrderValue,
                RevenuePeriodLabel = "Lifetime store revenue",
                TotalOrdersCount = totalOrders,
                PendingProcessingCount = pendingProcessing,
                FlaggedOrdersCount = flaggedCount,
                LowStockCount = lowStockCount,
                ChartDays = chartDays,
                ChartOrderCounts = chartCounts,
                RecentOrders = recentOrders,
                AttentionItems = attentionItems,
                TotalAttentionCount = totalAttentionCount,
                BestSellingMedicines = bestSellingList,
                TopCategories = topCategoriesList
            };

            return View(vm);
        }

        [HttpGet]
        [Route("Admin/Dashboard/Revenue")]
        public async Task<IActionResult> GetDashboardRevenue([FromQuery] string period = "all")
        {
            var nowUtc = DateTime.UtcNow;
            DateTime? startDate = null;
            DateTime? endDate = null;
            string label;

            switch (period?.ToLowerInvariant())
            {
                case "thismonth":
                    startDate = new DateTime(nowUtc.Year, nowUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                    label = nowUtc.ToString("MMMM yyyy");
                    break;
                case "lastmonth":
                    var firstOfThisMonth = new DateTime(nowUtc.Year, nowUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                    startDate = firstOfThisMonth.AddMonths(-1);
                    endDate = firstOfThisMonth;
                    label = startDate.Value.ToString("MMMM yyyy");
                    break;
                case "last3months":
                    startDate = nowUtc.AddMonths(-3);
                    label = "Last 3 months";
                    break;
                case "last6months":
                    startDate = nowUtc.AddMonths(-6);
                    label = "Last 6 months";
                    break;
                case "all":
                default:
                    startDate = null;
                    endDate = null;
                    label = "Lifetime store revenue";
                    break;
            }

            var query = _db.Orders.AsQueryable();
            if (startDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt < endDate.Value);
            }

            var orderCount = await query.CountAsync();
            var revenue = await query.SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;
            var avgOrder = orderCount > 0 ? revenue / orderCount : 0m;

            return Json(new
            {
                revenue = "৳" + revenue.ToString("#,##0"),
                rawRevenue = revenue,
                orderCount = orderCount,
                avgOrder = "৳" + avgOrder.ToString("#,##0"),
                label = label
            });
        }

        [HttpGet]
        public async Task<IActionResult> StockExpiry(string? filter)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var expiryThreshold = today.AddDays(30);

            var query = _db.Medicines
                .Include(m => m.Category)
                .Include(m => m.Stock)
                .AsQueryable();

            var allList = await query.ToListAsync();

            var rowList = allList.Select(m =>
            {
                var qty = m.Stock?.Quantity ?? 0;
                var exp = m.Stock?.ExpiryDate;
                var daysUntil = exp.HasValue
                    ? (exp.Value.ToDateTime(TimeOnly.MinValue) - DateTime.UtcNow.Date).Days
                    : 9999;

                string badge;
                string severity;

                if (qty == 0)
                {
                    badge = "Out of stock";
                    severity = "danger";
                }
                else if (daysUntil < 0)
                {
                    badge = "Expired";
                    severity = "danger";
                }
                else if (daysUntil <= 30)
                {
                    badge = "Expiring soon";
                    severity = "warning";
                }
                else if (qty < 10)
                {
                    badge = "Low stock";
                    severity = "warning";
                }
                else
                {
                    badge = "In stock";
                    severity = "success";
                }

                var percent = Math.Clamp((qty * 100) / 50, 0, 100);
                var progressColor = qty == 0 ? "red" : (qty < 10 ? "amber" : "green");

                return new AdminStockExpiryRowViewModel
                {
                    MedicineId = m.Id,
                    MedicineName = m.Name,
                    GenericName = m.GenericName,
                    CategoryName = m.Category?.Name ?? "Uncategorized",
                    Quantity = qty,
                    ExpiryDate = exp,
                    DaysUntilExpiry = daysUntil,
                    StatusBadge = badge,
                    StatusSeverity = severity,
                    ProgressPercent = percent,
                    ProgressColor = progressColor
                };
            }).ToList();

            var totalCount = rowList.Count;
            var expiringCount = rowList.Count(r => r.DaysUntilExpiry >= 0 && r.DaysUntilExpiry <= 30);
            var lowStockCount = rowList.Count(r => r.Quantity < 10 && r.Quantity > 0);
            var criticalCount = rowList.Count(r => r.Quantity == 0 || r.DaysUntilExpiry < 0);

            var activeFilter = filter ?? "All";
            var filtered = activeFilter switch
            {
                "ExpiringSoon" => rowList.Where(r => r.DaysUntilExpiry >= 0 && r.DaysUntilExpiry <= 30).ToList(),
                "LowStock" => rowList.Where(r => r.Quantity < 10).ToList(),
                "Critical" => rowList.Where(r => r.Quantity == 0 || r.DaysUntilExpiry < 0).ToList(),
                _ => rowList
            };

            filtered = filtered
                .OrderBy(r => r.ExpiryDate.HasValue ? 0 : 1)
                .ThenBy(r => r.ExpiryDate)
                .ThenBy(r => r.Quantity)
                .ToList();

            var vm = new AdminStockExpiryViewModel
            {
                Filter = activeFilter,
                TotalCount = totalCount,
                ExpiringSoonCount = expiringCount,
                LowStockCount = lowStockCount,
                CriticalCount = criticalCount,
                Items = filtered
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> AuditLog(string? actionFilter, string? adminId)
        {
            var query = _db.AuditLogs
                .Include(a => a.Admin)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(actionFilter) && actionFilter != "All")
            {
                query = query.Where(a => a.Action.Contains(actionFilter));
            }

            if (!string.IsNullOrWhiteSpace(adminId) && adminId != "All")
            {
                query = query.Where(a => a.AdminId == adminId);
            }

            var logs = await query
                .OrderByDescending(a => a.CreatedAt)
                .Take(150)
                .ToListAsync();

            var adminUsers = await _db.AuditLogs
                .Include(a => a.Admin)
                .Where(a => a.Admin != null)
                .Select(a => a.Admin!)
                .Distinct()
                .Select(u => new AdminUserOptionViewModel
                {
                    Id = u.Id,
                    Name = !string.IsNullOrWhiteSpace(u.FullName) ? u.FullName : u.UserName!
                })
                .ToListAsync();

            var mappedRows = logs.Select(l =>
            {
                var localTime = l.CreatedAt.ToLocalTime();
                var actionLower = l.Action.ToLowerInvariant();
                string actionType = "Other";

                if (actionLower.Contains("add") || actionLower.Contains("created")) actionType = "Add";
                else if (actionLower.Contains("edit") || actionLower.Contains("updated") || actionLower.Contains("marked")) actionType = "Edit";
                else if (actionLower.Contains("delete") || actionLower.Contains("removed")) actionType = "Delete";
                else if (actionLower.Contains("login") || actionLower.Contains("password") || actionLower.Contains("role")) actionType = "Security";

                string dateLabel;
                if (localTime.Date == DateTime.Now.Date)
                {
                    dateLabel = "Today";
                }
                else if (localTime.Date == DateTime.Now.Date.AddDays(-1))
                {
                    dateLabel = "Yesterday";
                }
                else
                {
                    dateLabel = localTime.ToString("dd MMM yyyy");
                }

                return new
                {
                    Row = new AuditLogRowViewModel
                    {
                        Id = l.Id,
                        Action = l.Action,
                        ActionType = actionType,
                        TableName = l.TableName,
                        RecordId = l.RecordId,
                        AdminName = l.Admin != null && !string.IsNullOrWhiteSpace(l.Admin.FullName) ? l.Admin.FullName : (l.Admin?.UserName ?? "System"),
                        AdminEmail = l.Admin?.Email ?? string.Empty,
                        CreatedAt = l.CreatedAt,
                        TimeString = localTime.ToString("h:mm tt")
                    },
                    DateLabel = dateLabel
                };
            }).ToList();

            var groups = mappedRows
                .GroupBy(x => x.DateLabel)
                .Select(g => new AuditLogGroupViewModel
                {
                    DateLabel = g.Key,
                    Entries = g.Select(x => x.Row).ToList()
                })
                .ToList();

            var vm = new AdminAuditLogViewModel
            {
                ActionFilter = actionFilter ?? "All",
                AdminIdFilter = adminId ?? "All",
                TotalCount = logs.Count,
                AdminOptions = adminUsers,
                Groups = groups
            };

            return View(vm);
        }


        // =====================
        // Contact Messages
        // =====================

                [HttpGet]
        public async Task<IActionResult> ContactMessages(string? status)
        {
            var query = _db.ContactMessages.AsQueryable();

            if (string.IsNullOrWhiteSpace(status) || status == "Unread")
            {
                query = query.Where(m => !m.IsRead);
                status = "Unread";
            }
            else if (status == "Read")
            {
                query = query.Where(m => m.IsRead);
            }
            // "All" — no filter

            var messages = await query
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => new AdminContactMessageRowViewModel
                {
                    Id = m.Id,
                    Name = m.Name,
                    Email = m.Email,
                    Message = m.Message,
                    IsRead = m.IsRead,
                    CreatedAt = m.CreatedAt
                })
                .ToListAsync();

            var unreadCount = await _db.ContactMessages.CountAsync(m => !m.IsRead);
            var totalCount = await _db.ContactMessages.CountAsync();

            var model = new AdminContactMessageListViewModel
            {
                Messages = messages,
                UnreadCount = unreadCount,
                TotalCount = totalCount,
                StatusFilter = status
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkMessageAsRead(int id)
        {
            var message = await _db.ContactMessages.FindAsync(id);

            if (message == null)
            {
                TempData["MessageError"] = "Message not found.";
                return RedirectToAction(nameof(ContactMessages));
            }

            message.IsRead = true;

            var adminId = _userManager.GetUserId(User);
            if (adminId != null)
            {
                _db.AuditLogs.Add(new AuditLog
                {
                    AdminId = adminId,
                    Action = $"Marked contact message #{message.Id} as read",
                    TableName = "ContactMessages",
                    RecordId = message.Id,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(ContactMessages));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkMessageAsUnread(int id)
        {
            var message = await _db.ContactMessages.FindAsync(id);

            if (message == null)
            {
                TempData["MessageError"] = "Message not found.";
                return RedirectToAction(nameof(ContactMessages));
            }

            message.IsRead = false;

            var adminId = _userManager.GetUserId(User);
            if (adminId != null)
            {
                _db.AuditLogs.Add(new AuditLog
                {
                    AdminId = adminId,
                    Action = $"Marked contact message #{message.Id} as unread",
                    TableName = "ContactMessages",
                    RecordId = message.Id,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(ContactMessages));
        }


        // =====================
        // Categories & SubCategories & Product Types
        // =====================

        [HttpGet]
        public async Task<IActionResult> Categories()
        {
            var model = await BuildCategoriesViewModelAsync();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetSubCategories(int categoryId)
        {
            var subCategories = await _db.SubCategories
                .Where(sc => sc.CategoryId == categoryId)
                .OrderBy(sc => sc.Name)
                .Select(sc => new SubCategoryOptionViewModel
                {
                    Id = sc.Id,
                    Label = sc.Name
                })
                .ToListAsync();

            return Json(subCategories);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(CategoryFormViewModel form)
        {
            if (!ModelState.IsValid)
            {
                TempData["CategoryError"] = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault() ?? "Please enter a valid name.";

                return RedirectToAction(nameof(Categories));
            }

            var name = form.Name.Trim();

            if (form.ParentCategoryId is null)
            {
                var duplicate = await _db.Categories.AnyAsync(c =>
                    c.Name.ToLower() == name.ToLower());

                if (duplicate)
                {
                    TempData["CategoryError"] = $"'{name}' already exists as a category.";
                    return RedirectToAction(nameof(Categories));
                }

                _db.Categories.Add(new Category
                {
                    Name = name,
                    Description = string.IsNullOrWhiteSpace(form.Description) ? null : form.Description.Trim(),
                    CreatedAt = DateTime.UtcNow
                });

                TempData["CategorySuccess"] = $"Category '{name}' added.";
            }
            else
            {
                var parentExists = await _db.Categories.AnyAsync(c => c.Id == form.ParentCategoryId.Value);
                if (!parentExists)
                {
                    TempData["CategoryError"] = "Selected parent category no longer exists — refresh the page.";
                    return RedirectToAction(nameof(Categories));
                }

                var duplicate = await _db.SubCategories.AnyAsync(sc =>
                    sc.CategoryId == form.ParentCategoryId.Value &&
                    sc.Name.ToLower() == name.ToLower());

                if (duplicate)
                {
                    TempData["CategoryError"] = $"'{name}' already exists under this category.";
                    return RedirectToAction(nameof(Categories));
                }

                _db.SubCategories.Add(new SubCategory
                {
                    CategoryId = form.ParentCategoryId.Value,
                    Name = name,
                    Description = string.IsNullOrWhiteSpace(form.Description) ? null : form.Description.Trim(),
                    CreatedAt = DateTime.UtcNow
                });

                TempData["CategorySuccess"] = $"Subcategory '{name}' added.";
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Categories));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(CategoryFormViewModel form)
        {
            if (form.Id is null || string.IsNullOrEmpty(form.Kind) || !ModelState.IsValid)
            {
                TempData["CategoryError"] = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault() ?? "Please enter a valid name.";

                return RedirectToAction(nameof(Categories));
            }

            var name = form.Name.Trim();

            var wouldChangeKind = form.Kind == "category"
                ? form.ParentCategoryId.HasValue
                : !form.ParentCategoryId.HasValue;

            if (wouldChangeKind)
            {
                TempData["CategoryError"] =
                    "Can't turn a category into a subcategory (or back) by editing. Delete it and add it again instead.";
                return RedirectToAction(nameof(Categories));
            }

            if (form.Kind == "category")
            {
                var category = await _db.Categories.FindAsync(form.Id.Value);
                if (category == null)
                {
                    TempData["CategoryError"] = "Category not found.";
                    return RedirectToAction(nameof(Categories));
                }

                var duplicate = await _db.Categories.AnyAsync(c =>
                    c.Id != category.Id && c.Name.ToLower() == name.ToLower());

                if (duplicate)
                {
                    TempData["CategoryError"] = $"'{name}' already exists as a category.";
                    return RedirectToAction(nameof(Categories));
                }

                category.Name = name;
                category.Description = string.IsNullOrWhiteSpace(form.Description) ? null : form.Description.Trim();
            }
            else
            {
                var subCategory = await _db.SubCategories.FindAsync(form.Id.Value);
                if (subCategory == null)
                {
                    TempData["CategoryError"] = "Subcategory not found.";
                    return RedirectToAction(nameof(Categories));
                }

                var duplicate = await _db.SubCategories.AnyAsync(sc =>
                    sc.Id != subCategory.Id &&
                    sc.CategoryId == form.ParentCategoryId!.Value &&
                    sc.Name.ToLower() == name.ToLower());

                if (duplicate)
                {
                    TempData["CategoryError"] = $"'{name}' already exists under this category.";
                    return RedirectToAction(nameof(Categories));
                }

                subCategory.Name = name;
                subCategory.Description = string.IsNullOrWhiteSpace(form.Description) ? null : form.Description.Trim();
                subCategory.CategoryId = form.ParentCategoryId!.Value;
            }

            await _db.SaveChangesAsync();
            TempData["CategorySuccess"] = $"'{name}' updated.";
            return RedirectToAction(nameof(Categories));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _db.Categories
                .Include(c => c.SubCategories)
                .Include(c => c.Medicines)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                TempData["CategoryError"] = "Category not found.";
                return RedirectToAction(nameof(Categories));
            }

            if (category.SubCategories.Count > 0)
            {
                TempData["CategoryError"] =
                    $"Cannot delete '{category.Name}' — it has {category.SubCategories.Count} " +
                    $"subcategor{(category.SubCategories.Count == 1 ? "y" : "ies")}. Delete those first.";
                return RedirectToAction(nameof(Categories));
            }

            if (category.Medicines.Count > 0)
            {
                TempData["CategoryError"] =
                    $"Cannot delete '{category.Name}' — {category.Medicines.Count} medicine(s) still use it.";
                return RedirectToAction(nameof(Categories));
            }

            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();

            TempData["CategorySuccess"] = $"Category '{category.Name}' deleted.";
            return RedirectToAction(nameof(Categories));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSubCategory(int id)
        {
            var subCategory = await _db.SubCategories
                .Include(sc => sc.Medicines)
                .FirstOrDefaultAsync(sc => sc.Id == id);

            if (subCategory == null)
            {
                TempData["CategoryError"] = "Subcategory not found.";
                return RedirectToAction(nameof(Categories));
            }

            if (subCategory.Medicines.Count > 0)
            {
                TempData["CategoryError"] =
                    $"Cannot delete '{subCategory.Name}' — {subCategory.Medicines.Count} medicine(s) still use it.";
                return RedirectToAction(nameof(Categories));
            }

            _db.SubCategories.Remove(subCategory);
            await _db.SaveChangesAsync();

            TempData["CategorySuccess"] = $"Subcategory '{subCategory.Name}' deleted.";
            return RedirectToAction(nameof(Categories));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProductType(
            ProductTypeFormViewModel form)
        {
            if (!ModelState.IsValid)
            {
                TempData["ProductTypeError"] =
                    "Please enter a valid product type name.";

                return RedirectToAction(nameof(Categories));
            }

            var duplicate =
                await _db.ProductTypes.AnyAsync(p =>
                    p.Name.ToLower() ==
                    form.Name.Trim().ToLower());

            if (duplicate)
            {
                TempData["ProductTypeError"] =
                    $"'{form.Name}' already exists.";

                return RedirectToAction(nameof(Categories));
            }

            _db.ProductTypes.Add(new ProductType
            {
                Name = form.Name.Trim()
            });

            await _db.SaveChangesAsync();

            TempData["ProductTypeSuccess"] =
                $"Product type '{form.Name}' added.";

            return RedirectToAction(nameof(Categories));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProductType(
            ProductTypeFormViewModel form)
        {
            if (form.Id is null || !ModelState.IsValid)
            {
                TempData["ProductTypeError"] =
                    "Please enter a valid product type name.";

                return RedirectToAction(nameof(Categories));
            }

            var productType =
                await _db.ProductTypes.FindAsync(form.Id.Value);

            if (productType == null)
            {
                TempData["ProductTypeError"] =
                    "Product type not found.";

                return RedirectToAction(nameof(Categories));
            }

            var duplicate =
                await _db.ProductTypes.AnyAsync(p =>
                    p.Id != productType.Id &&
                    p.Name.ToLower() ==
                    form.Name.Trim().ToLower());

            if (duplicate)
            {
                TempData["ProductTypeError"] =
                    $"'{form.Name}' already exists.";

                return RedirectToAction(nameof(Categories));
            }

            productType.Name = form.Name.Trim();

            await _db.SaveChangesAsync();

            TempData["ProductTypeSuccess"] =
                "Product type updated.";

            return RedirectToAction(nameof(Categories));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProductType(int id)
        {
            var productType = await _db.ProductTypes
                .Include(p => p.Medicines)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (productType == null)
            {
                TempData["ProductTypeError"] =
                    "Product type not found.";

                return RedirectToAction(nameof(Categories));
            }

            if (productType.Medicines.Count > 0)
            {
                TempData["ProductTypeError"] =
                    $"Cannot delete '{productType.Name}' — " +
                    $"{productType.Medicines.Count} medicine(s) still use it.";

                return RedirectToAction(nameof(Categories));
            }

            _db.ProductTypes.Remove(productType);
            await _db.SaveChangesAsync();

            TempData["ProductTypeSuccess"] =
                $"Product type '{productType.Name}' deleted.";

            return RedirectToAction(nameof(Categories));
        }


        // =====================
        // Add Medicine
        // =====================

        [HttpGet]
        public async Task<IActionResult> AddMedicine()
        {
            var model = new AdminMedicineFormPageViewModel
            {
                Form = new MedicineFormViewModel
                {
                    ExpiryDate =
                        DateOnly.FromDateTime(
                            DateTime.UtcNow.AddYears(2))
                },
                CategoryOptions =
                    await BuildCategoryDropdownOptionsAsync(),
                ProductTypeOptions =
                    await BuildProductTypeDropdownOptionsAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMedicine(
            AdminMedicineFormPageViewModel model)
        {
            var form = model.Form;

            form.SideEffects = form.SideEffects
                .Where(se =>
                    !string.IsNullOrWhiteSpace(se.Effect))
                .ToList();

            string? finalImageUrl = form.ImageUrl;

            if (form.ImageFile != null &&
                form.ImageFile.Length > 0)
            {
                if (!IsValidImageFile(form.ImageFile))
                {
                    ModelState.AddModelError(
                        "Form.ImageFile",
                        "Upload a JPG, PNG, or WEBP image under 5MB.");
                }
                else
                {
                    try
                    {
                        finalImageUrl =
                            await _imageUploadService
                                .UploadMedicineImageAsync(
                                    form.ImageFile);
                    }
                    catch (InvalidOperationException ex)
                    {
                        ModelState.AddModelError(
                            "Form.ImageFile",
                            ex.Message);
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                model.CategoryOptions =
                    await BuildCategoryDropdownOptionsAsync();

                model.ProductTypeOptions =
                    await BuildProductTypeDropdownOptionsAsync();

                return View(model);
            }

            var categoryExists =
                await _db.Categories.AnyAsync(c =>
                    c.Id == form.CategoryId);

            var productTypeExists =
                await _db.ProductTypes.AnyAsync(p =>
                    p.Id == form.ProductTypeId);

            if (!categoryExists ||
                !productTypeExists)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The selected category or product type " +
                    "no longer exists — refresh the page.");

                model.CategoryOptions =
                    await BuildCategoryDropdownOptionsAsync();

                model.ProductTypeOptions =
                    await BuildProductTypeDropdownOptionsAsync();

                return View(model);
            }

            if (form.SubCategoryId.HasValue)
            {
                var subCategoryValid = await _db.SubCategories.AnyAsync(sc =>
                    sc.Id == form.SubCategoryId.Value &&
                    sc.CategoryId == form.CategoryId);

                if (!subCategoryValid)
                {
                    ModelState.AddModelError(
                        "Form.SubCategoryId",
                        "The selected subcategory doesn't belong to the chosen category.");

                    model.CategoryOptions = await BuildCategoryDropdownOptionsAsync();
                    model.ProductTypeOptions = await BuildProductTypeDropdownOptionsAsync();
                    return View(model);
                }
            }

            var medicine = new Medicine
            {
                Name = form.Name.Trim(),
                CategoryId = form.CategoryId,
                SubCategoryId = form.SubCategoryId,
                ProductTypeId = form.ProductTypeId,

                Manufacturer =
                    string.IsNullOrWhiteSpace(form.Manufacturer)
                        ? null
                        : form.Manufacturer.Trim(),

                GenericName =
                    string.IsNullOrWhiteSpace(form.GenericName)
                        ? null
                        : form.GenericName.Trim(),

                Unit =
                    string.IsNullOrWhiteSpace(form.Unit)
                        ? null
                        : form.Unit.Trim(),

                Description =
                    string.IsNullOrWhiteSpace(form.Description)
                        ? null
                        : form.Description.Trim(),

                Dosage =
                    string.IsNullOrWhiteSpace(form.Dosage)
                        ? null
                        : form.Dosage.Trim(),

                ImageUrl =
                    string.IsNullOrWhiteSpace(finalImageUrl)
                        ? null
                        : finalImageUrl.Trim(),

                Price = form.Price,
                RequiresPrescription = form.RequiresPrescription,

                SensitivityLevel =
                    string.IsNullOrWhiteSpace(form.SensitivityLevel)
                        ? null
                        : form.SensitivityLevel,

                CreatedAt = DateTime.UtcNow,

                Stock = new Stock
                {
                    Quantity = form.StockQuantity,
                    ExpiryDate = form.ExpiryDate,
                    UpdatedAt = DateTime.UtcNow
                },

                SideEffects = form.SideEffects
                    .Select(se => new SideEffect
                    {
                        Effect = se.Effect.Trim(),
                        Severity = se.Severity
                    })
                    .ToList()
            };

            _db.Medicines.Add(medicine);
            await _db.SaveChangesAsync();

            TempData["MedicineSuccess"] =
                $"Medicine '{medicine.Name}' added.";

            return RedirectToAction(nameof(Medicines));
        }


        // =====================
        // Edit Medicine
        // =====================

        [HttpGet]
        public async Task<IActionResult> EditMedicine(int id)
        {
            var medicine = await _db.Medicines
                .Include(m => m.Stock)
                .Include(m => m.SideEffects)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (medicine == null)
            {
                TempData["MedicineError"] =
                    "Medicine not found.";

                return RedirectToAction(nameof(AddMedicine));
            }

            var form = new MedicineFormViewModel
            {
                Id = medicine.Id,
                Name = medicine.Name,
                CategoryId = medicine.CategoryId,
                SubCategoryId = medicine.SubCategoryId,
                ProductTypeId = medicine.ProductTypeId,
                Manufacturer = medicine.Manufacturer,
                Price = medicine.Price,

                StockQuantity =
                    medicine.Stock?.Quantity ?? 0,

                ExpiryDate =
                    medicine.Stock?.ExpiryDate ??
                    DateOnly.FromDateTime(
                        DateTime.UtcNow.AddYears(2)),

                RequiresPrescription =
                    medicine.RequiresPrescription,

                GenericName = medicine.GenericName,
                Unit = medicine.Unit,
                Description = medicine.Description,
                Dosage = medicine.Dosage,
                ImageUrl = medicine.ImageUrl,
                SensitivityLevel = medicine.SensitivityLevel,

                SideEffects = medicine.SideEffects
                    .Select(se =>
                        new SideEffectFormViewModel
                        {
                            Effect = se.Effect,
                            Severity = se.Severity
                        })
                    .ToList()
            };

            var model =
                new AdminMedicineFormPageViewModel
                {
                    Form = form,
                    CategoryOptions =
                        await BuildCategoryDropdownOptionsAsync(),
                    ProductTypeOptions =
                        await BuildProductTypeDropdownOptionsAsync()
                };

            return View("AddMedicine", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMedicine(
            MedicineFormViewModel form)
        {
            form.SideEffects = form.SideEffects
                .Where(se =>
                    !string.IsNullOrWhiteSpace(se.Effect))
                .ToList();

            if (form.Id is null)
            {
                TempData["MedicineError"] =
                    "Missing medicine id.";

                return RedirectToAction(nameof(AddMedicine));
            }

            string? finalImageUrl = form.ImageUrl;

            if (form.ImageFile != null &&
                form.ImageFile.Length > 0)
            {
                if (!IsValidImageFile(form.ImageFile))
                {
                    ModelState.AddModelError(
                        nameof(form.ImageFile),
                        "Upload a JPG, PNG, or WEBP image under 5MB.");
                }
                else
                {
                    try
                    {
                        finalImageUrl =
                            await _imageUploadService
                                .UploadMedicineImageAsync(
                                    form.ImageFile);
                    }
                    catch (InvalidOperationException ex)
                    {
                        ModelState.AddModelError(
                            nameof(form.ImageFile),
                            ex.Message);
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                var invalidModel =
                    new AdminMedicineFormPageViewModel
                    {
                        Form = form,
                        CategoryOptions =
                            await BuildCategoryDropdownOptionsAsync(),
                        ProductTypeOptions =
                            await BuildProductTypeDropdownOptionsAsync()
                    };

                return View("AddMedicine", invalidModel);
            }

            var medicine = await _db.Medicines
                .Include(m => m.Stock)
                .Include(m => m.SideEffects)
                .FirstOrDefaultAsync(m =>
                    m.Id == form.Id.Value);

            if (medicine == null)
            {
                TempData["MedicineError"] =
                    "Medicine not found — it may have been deleted.";

                return RedirectToAction(nameof(AddMedicine));
            }

            var categoryExists =
                await _db.Categories.AnyAsync(c =>
                    c.Id == form.CategoryId);

            var productTypeExists =
                await _db.ProductTypes.AnyAsync(p =>
                    p.Id == form.ProductTypeId);

            if (!categoryExists ||
                !productTypeExists)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The selected category or product type " +
                    "no longer exists — refresh the page.");

                var invalidModel =
                    new AdminMedicineFormPageViewModel
                    {
                        Form = form,
                        CategoryOptions =
                            await BuildCategoryDropdownOptionsAsync(),
                        ProductTypeOptions =
                            await BuildProductTypeDropdownOptionsAsync()
                    };

                return View("AddMedicine", invalidModel);
            }

            if (form.SubCategoryId.HasValue)
            {
                var subCategoryValid = await _db.SubCategories.AnyAsync(sc =>
                    sc.Id == form.SubCategoryId.Value &&
                    sc.CategoryId == form.CategoryId);

                if (!subCategoryValid)
                {
                    ModelState.AddModelError(
                        nameof(form.SubCategoryId),
                        "The selected subcategory doesn't belong to the chosen category.");

                    var invalidModel =
                        new AdminMedicineFormPageViewModel
                        {
                            Form = form,
                            CategoryOptions =
                                await BuildCategoryDropdownOptionsAsync(),
                            ProductTypeOptions =
                                await BuildProductTypeDropdownOptionsAsync()
                        };

                    return View("AddMedicine", invalidModel);
                }
            }

            medicine.Name = form.Name.Trim();
            medicine.CategoryId = form.CategoryId;
            medicine.SubCategoryId = form.SubCategoryId;
            medicine.ProductTypeId = form.ProductTypeId;

            medicine.Manufacturer =
                string.IsNullOrWhiteSpace(form.Manufacturer)
                    ? null
                    : form.Manufacturer.Trim();

            medicine.GenericName =
                string.IsNullOrWhiteSpace(form.GenericName)
                    ? null
                    : form.GenericName.Trim();

            medicine.Unit =
                string.IsNullOrWhiteSpace(form.Unit)
                    ? null
                    : form.Unit.Trim();

            medicine.Description =
                string.IsNullOrWhiteSpace(form.Description)
                    ? null
                    : form.Description.Trim();

            medicine.Dosage =
                string.IsNullOrWhiteSpace(form.Dosage)
                    ? null
                    : form.Dosage.Trim();

            medicine.ImageUrl =
                string.IsNullOrWhiteSpace(finalImageUrl)
                    ? null
                    : finalImageUrl.Trim();

            medicine.Price = form.Price;
            medicine.RequiresPrescription = form.RequiresPrescription;

            medicine.SensitivityLevel =
                string.IsNullOrWhiteSpace(
                    form.SensitivityLevel)
                    ? null
                    : form.SensitivityLevel;

            if (medicine.Stock != null)
            {
                medicine.Stock.Quantity = form.StockQuantity;
                medicine.Stock.ExpiryDate = form.ExpiryDate;
                medicine.Stock.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                medicine.Stock = new Stock
                {
                    Quantity = form.StockQuantity,
                    ExpiryDate = form.ExpiryDate,
                    UpdatedAt = DateTime.UtcNow
                };
            }

            _db.SideEffects.RemoveRange(medicine.SideEffects);

            medicine.SideEffects =
                form.SideEffects
                    .Select(se => new SideEffect
                    {
                        Effect = se.Effect.Trim(),
                        Severity = se.Severity
                    })
                    .ToList();

            await _db.SaveChangesAsync();

            TempData["MedicineSuccess"] =
                $"Medicine '{medicine.Name}' updated.";

            return RedirectToAction(nameof(Medicines));
        }


        // =====================
        // Delete Medicine
        // =====================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMedicine(int id)
        {
            var medicine = await _db.Medicines
                .Include(m => m.OrderItems)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (medicine == null)
            {
                TempData["MedicineError"] =
                    "Medicine not found.";

                return RedirectToAction(nameof(AddMedicine));
            }

            if (medicine.OrderItems.Count > 0)
            {
                TempData["MedicineError"] =
                    $"Cannot delete '{medicine.Name}' — " +
                    $"it appears in {medicine.OrderItems.Count} " +
                    "past order(s). Deleting it would corrupt order history.";

                return RedirectToAction(
                    nameof(EditMedicine),
                    new { id = medicine.Id });
            }

            _db.Medicines.Remove(medicine);
            await _db.SaveChangesAsync();

            TempData["MedicineSuccess"] =
                $"Medicine '{medicine.Name}' deleted.";

            return RedirectToAction(nameof(AddMedicine));
        }


        // =====================
        // Helpers
        // =====================

        private async Task<AdminCategoriesViewModel> BuildCategoriesViewModelAsync()
        {
            var categories = await _db.Categories
                .Include(c => c.SubCategories)
                .Include(c => c.Medicines)
                .OrderBy(c => c.Name)
                .ToListAsync();

            var subCategories = await _db.SubCategories
                .Include(sc => sc.Category)
                .Include(sc => sc.Medicines)
                .OrderBy(sc => sc.Name)
                .ToListAsync();

            var productTypes = await _db.ProductTypes
                .Include(p => p.Medicines)
                .OrderBy(p => p.Name)
                .ToListAsync();

            return new AdminCategoriesViewModel
            {
                Categories = categories.Select(c => new CategoryRowViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    SubCategoryCount = c.SubCategories.Count,
                    MedicineCount = c.Medicines.Count
                }).ToList(),

                SubCategories = subCategories.Select(sc => new SubCategoryRowViewModel
                {
                    Id = sc.Id,
                    Name = sc.Name,
                    Description = sc.Description,
                    CategoryId = sc.CategoryId,
                    CategoryName = sc.Category.Name,
                    MedicineCount = sc.Medicines.Count
                }).ToList(),

                ProductTypes = productTypes.Select(p => new ProductTypeRowViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    MedicineCount = p.Medicines.Count
                }).ToList(),

                ParentCategoryOptions = categories.Select(c => new CategoryOptionViewModel
                {
                    Id = c.Id,
                    Name = c.Name
                }).ToList()
            };
        }

        private async Task<List<DropdownOptionViewModel>>
            BuildCategoryDropdownOptionsAsync()
        {
            return await _db.Categories
                .OrderBy(c => c.Name)
                .Select(c => new DropdownOptionViewModel
                {
                    Id = c.Id,
                    Label = c.Name
                })
                .ToListAsync();
        }

        private async Task<List<DropdownOptionViewModel>>
            BuildProductTypeDropdownOptionsAsync()
        {
            return await _db.ProductTypes
                .OrderBy(p => p.Name)
                .Select(p =>
                    new DropdownOptionViewModel
                    {
                        Id = p.Id,
                        Label = p.Name
                    })
                .ToListAsync();
        }

        private static bool IsValidImageFile(
            IFormFile file)
        {
            var allowedTypes = new[]
            {
                "image/jpeg",
                "image/png",
                "image/webp"
            };

            const long maxBytes =
                5 * 1024 * 1024; // 5 MB

            return allowedTypes.Contains(
                       file.ContentType)
                   &&
                   file.Length <= maxBytes;
        }
    }
}
using System;
using System.Collections.Generic;

namespace MediCart.Web.Models
{
    public class AdminDashboardViewModel
    {
        // KPI Stat Cards
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public string RevenuePeriodLabel { get; set; } = "Lifetime store revenue";
        public int TotalOrdersCount { get; set; }
        public int PendingProcessingCount { get; set; }
        public int FlaggedOrdersCount { get; set; }
        public int LowStockCount { get; set; }

        // Chart Data (Last 7 Days)
        public List<string> ChartDays { get; set; } = new();
        public List<int> ChartOrderCounts { get; set; } = new();

        // Recent Orders Table
        public List<DashboardRecentOrderViewModel> RecentOrders { get; set; } = new();

        // Attention Needed
        public int TotalAttentionCount { get; set; }
        public List<DashboardAttentionItemViewModel> AttentionItems { get; set; } = new();

        // Best Selling Medicines
        public List<DashboardBestSellingMedicineViewModel> BestSellingMedicines { get; set; } = new();

        // Top Categories
        public List<DashboardTopCategoryViewModel> TopCategories { get; set; } = new();
    }

    public class DashboardBestSellingMedicineViewModel
    {
        public int MedicineId { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public int UnitsSold { get; set; }
        public decimal Revenue { get; set; }
        public int Rank { get; set; }
    }

    public class DashboardTopCategoryViewModel
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public decimal Percentage { get; set; }
    }

    public class DashboardRecentOrderViewModel
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class DashboardAttentionItemViewModel
    {
        public string Type { get; set; } = string.Empty; // FlaggedOrder, ExpiringSoon, LowStock
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string Severity { get; set; } = "warning"; // danger, warning, neutral
        public string ActionUrl { get; set; } = string.Empty;
        public string ActionText { get; set; } = "Review";
    }
}

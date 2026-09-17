using System;
using System.Collections.Generic;

namespace MediCart.Web.Models
{
    public class AdminStockExpiryViewModel
    {
        public string Filter { get; set; } = "All"; // All, ExpiringSoon, LowStock, Critical
        public int TotalCount { get; set; }
        public int ExpiringSoonCount { get; set; }
        public int LowStockCount { get; set; }
        public int CriticalCount { get; set; }

        public List<AdminStockExpiryRowViewModel> Items { get; set; } = new();
    }

    public class AdminStockExpiryRowViewModel
    {
        public int MedicineId { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public string GenericName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateOnly? ExpiryDate { get; set; }
        public int DaysUntilExpiry { get; set; }
        public string StatusBadge { get; set; } = string.Empty;
        public string StatusSeverity { get; set; } = "success"; // success, warning, danger
        public int ProgressPercent { get; set; } // 0 to 100
        public string ProgressColor { get; set; } = "green"; // green, amber, red
    }
}

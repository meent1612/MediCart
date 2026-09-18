namespace MediCart.Web.Models
{
    public class MedicineListRowViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string CategoryName { get; set; } = "";
        public string? SubCategoryName { get; set; }
        public string ProductTypeName { get; set; } = "";
        public string? Manufacturer { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public DateOnly ExpiryDate { get; set; }
        public string? SensitivityLevel { get; set; } // low | mid | high | null
        public bool RequiresPrescription { get; set; }

        public bool IsLowStock => StockQuantity < 10;
        public int DaysUntilExpiry => (ExpiryDate.ToDateTime(TimeOnly.MinValue) - DateTime.UtcNow.Date).Days;
        public bool IsExpiringSoon => DaysUntilExpiry <= 30 && DaysUntilExpiry >= 0;
        public bool IsExpired => DaysUntilExpiry < 0;
        public bool IsNearExpiryWindow => DaysUntilExpiry <= 60 && DaysUntilExpiry > 30;
    }

    public class AdminMedicinesPageViewModel
    {
        public List<MedicineListRowViewModel> Medicines { get; set; } = new();
        public List<DropdownOptionViewModel> CategoryOptions { get; set; } = new();
        public List<DropdownOptionViewModel> ProductTypeOptions { get; set; } = new();

        // Reflects what was actually applied, so the filter bar can show current state
        public string? Search { get; set; }
        public int? CategoryId { get; set; }
        public int? SubCategoryId { get; set; }
        public int? ProductTypeId { get; set; }

        public int TotalCount { get; set; }
    }
}
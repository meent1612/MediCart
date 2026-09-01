namespace MediCart.Web.Models
{
    public class MedicineListRowViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string CategoryName { get; set; } = "";
        public string ProductTypeName { get; set; } = "";
        public string? Manufacturer { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public DateOnly ExpiryDate { get; set; }
        public string? SensitivityLevel { get; set; } // low | mid | high | null
        public bool RequiresPrescription { get; set; }

        public bool IsLowStock => StockQuantity <= 10; // matches the low-stock threshold used in Report 01/02
        public bool IsExpiringSoon => ExpiryDate <= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));
        public bool IsExpired => ExpiryDate < DateOnly.FromDateTime(DateTime.UtcNow);
    }

    public class AdminMedicinesPageViewModel
    {
        public List<MedicineListRowViewModel> Medicines { get; set; } = new();
        public List<DropdownOptionViewModel> CategoryOptions { get; set; } = new();
        public List<DropdownOptionViewModel> ProductTypeOptions { get; set; } = new();

        // Reflects what was actually applied, so the filter bar can show current state
        public string? Search { get; set; }
        public int? CategoryId { get; set; }
        public int? ProductTypeId { get; set; }

        public int TotalCount { get; set; }
    }
}
using MediCart.Web.Services;

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
        public string? SensitivityLevel { get; set; }
        public bool RequiresPrescription { get; set; }

        // Computed — all use StockExpiryHelper constants
        public int DaysUntilExpiry =>
            (ExpiryDate.ToDateTime(TimeOnly.MinValue) - DateTime.UtcNow.Date).Days;

        public bool IsOutOfStock  => StockExpiryHelper.IsOutOfStock(StockQuantity);
        public bool IsLowStock    => StockExpiryHelper.IsLowStock(StockQuantity);
        public bool IsExpired     => StockExpiryHelper.IsExpired(DaysUntilExpiry);
        public bool IsCriticalExpiry => StockExpiryHelper.IsCriticalExpiry(DaysUntilExpiry);
        public bool IsExpiringSoon   => StockExpiryHelper.IsWarningExpiry(DaysUntilExpiry);

        // IsNearExpiryWindow was used in the old Medicines view for a softer
        // colour — kept for backward compat but now maps to IsExpiringSoon.
        public bool IsNearExpiryWindow => IsExpiringSoon;
    }

    public class AdminMedicinesPageViewModel
    {
        public List<MedicineListRowViewModel> Medicines { get; set; } = new();
        public List<DropdownOptionViewModel> CategoryOptions { get; set; } = new();
        public List<DropdownOptionViewModel> ProductTypeOptions { get; set; } = new();

        public string? Search { get; set; }
        public int? CategoryId { get; set; }
        public int? SubCategoryId { get; set; }
        public int? ProductTypeId { get; set; }

        public int TotalCount { get; set; }
    }
}
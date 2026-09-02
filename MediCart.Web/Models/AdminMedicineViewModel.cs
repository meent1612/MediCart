using System.ComponentModel.DataAnnotations;

namespace MediCart.Web.Models
{
    public class DropdownOptionViewModel
    {
        public int Id { get; set; }
        public string Label { get; set; } = "";
    }

    public class SideEffectFormViewModel
    {
        [Required(ErrorMessage = "Enter a side effect")]
        [StringLength(150, MinimumLength = 2)]
        public string Effect { get; set; } = "";

        [Required]
        public string Severity { get; set; } = "mild"; // mild | moderate | severe — matches DB CHECK constraint
    }

    public class MedicineFormViewModel
    {
        // Null on Add. Step 16 (Edit) will reuse this same form/view with Id set.
        public int? Id { get; set; }
        public IFormFile? ImageFile { get; set; }
        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Enter the medicine name")]
        [StringLength(150, MinimumLength = 2)]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Select a category")]
        [Range(1, int.MaxValue, ErrorMessage = "Select a category")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Select a product type")]
        [Range(1, int.MaxValue, ErrorMessage = "Select a product type")]
        public int ProductTypeId { get; set; }

        [StringLength(100)]
        public string? Manufacturer { get; set; }
                [StringLength(150)]
        public string? GenericName { get; set; }

        [StringLength(30)]
        public string? Unit { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Enter a price")]
        [Range(0.01, 100000, ErrorMessage = "Enter a price between 0.01 and 100,000")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Enter the starting stock quantity")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
        public int StockQuantity { get; set; }

        [Required(ErrorMessage = "Enter an expiry date")]
        [DataType(DataType.Date)]
        public DateOnly ExpiryDate { get; set; }

        public bool RequiresPrescription { get; set; }

        // "" / null = not a sensitive medicine; otherwise "low" | "mid" | "high"
        public string? SensitivityLevel { get; set; }

        public List<SideEffectFormViewModel> SideEffects { get; set; } = new();
    }

    public class AdminMedicineFormPageViewModel
    {
        public MedicineFormViewModel Form { get; set; } = new();
        public List<DropdownOptionViewModel> CategoryOptions { get; set; } = new();
        public List<DropdownOptionViewModel> ProductTypeOptions { get; set; } = new();
    }
}
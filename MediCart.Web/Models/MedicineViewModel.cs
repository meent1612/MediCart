using System.Collections.Generic;

namespace MediCart.Web.Models
{
    public enum StockStatus
    {
        InStock,
        LowStock,
        OutOfStock
    }

    public class MedicineViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string GenericName { get; set; } = string.Empty;   // e.g. "Omeprazole 20mg"
        public string Manufacturer { get; set; } = string.Empty;  // e.g. "Square Pharma"
        public string ProductType { get; set; } = string.Empty;   // Tablet, Syrup, Injection, Ointment, Drops
        public string Category { get; set; } = string.Empty;      // Pain relief, Gastric, Fever & cold, Allergy, Vitamins
        public List<string> UseTags { get; set; } = new();        // Fever, Acidity, Headache, Cough
        public decimal Price { get; set; }                        // in BDT (Taka)
        public StockStatus Stock { get; set; }
        public string Intensity { get; set; } = "Mild";           // Mild, Moderate, Strong
        public bool RequiresPrescription { get; set; }
        public string Description { get; set; } = string.Empty;
        public string DosageInstructions { get; set; } = string.Empty;
        public List<string> SideEffects { get; set; } = new();
        public string IconGlyph { get; set; } = "💊";              // swap for a real image path later

        public string StockLabel => Stock switch
        {
            StockStatus.InStock => "In stock",
            StockStatus.LowStock => "Low stock",
            _ => "Out of stock"
        };

        public string StockCss => Stock switch
        {
            StockStatus.InStock => "badge-instock",
            StockStatus.LowStock => "badge-lowstock",
            _ => "badge-outstock"
        };
    }
}

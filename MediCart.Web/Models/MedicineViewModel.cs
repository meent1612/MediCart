using System.Collections.Generic;

namespace MediCart.Web.Models
{
    public class MedicineViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Composition { get; set; } = "";
        public string Strength { get; set; } = "";
        public string Manufacturer { get; set; } = "";
        public string ProductType { get; set; } = "";   // Tablet, Syrup, Injection, Ointment, Drops
        public string Category { get; set; } = "";      // Pain relief, Gastric, Fever & cold, Allergy, Vitamins
        public List<string> UseTags { get; set; } = new(); // Fever, Acidity, Headache, Cough
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool RequiresRx { get; set; }
        public string Potency { get; set; } = "Mild";   // Mild, Strong
        public int Popularity { get; set; }
        public string About { get; set; } = "";
        public string Dosage { get; set; } = "";
        public List<string> SideEffects { get; set; } = new();

        public string StockStatus =>
            Stock <= 0 ? "Out of stock" : Stock <= 30 ? "Low stock" : "In stock";

        public string StockCssClass =>
            Stock <= 0 ? "out" : Stock <= 30 ? "low" : "in";
    }
}

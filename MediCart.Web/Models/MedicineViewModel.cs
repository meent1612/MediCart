using System.Collections.Generic;

namespace MediCart.Web.Models
{
    public class MedicineViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Composition { get; set; } = "";
        public string Manufacturer { get; set; } = "";
        public string ProductType { get; set; } = "";   // Tablet, Syrup, Injection, Ointment, Drops
        public string Category { get; set; } = "";      // Pain relief, Gastric, Fever & cold, Allergy, Vitamins
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool RequiresRx { get; set; }
        public string About { get; set; } = "";
        public List<string> SideEffects { get; set; } = new();
        public string? ImageUrl { get; set; } // from Cloudinary via Medicine.ImageUrl; null = show placeholder icon

        // TODO(backend): these fields have no matching column in the current
        // Medicine table. Left nullable on purpose so the UI can show a clear
        // "not available yet" state instead of faking data. If/when these are
        // added to the schema, wire them up in MedicinesController.Index().
        public string? Strength { get; set; }      // e.g. "500mg+65mg" — no column yet
        public string? Dosage { get; set; }         // separate from Description — no column yet
        public string? Potency { get; set; }        // Mild/Strong — NOT the same as SensitivityLevel (order-flagging tier)
        public int? Popularity { get; set; }         // needed for "Most popular" sort — no column yet
        public List<string> UseTags { get; set; } = new(); // Fever, Acidity, Headache, Cough — no table yet

        public string StockStatus =>
            Stock <= 0 ? "Out of stock" : Stock <= 30 ? "Low stock" : "In stock";

        public string StockCssClass =>
            Stock <= 0 ? "out" : Stock <= 30 ? "low" : "in";
    }
}

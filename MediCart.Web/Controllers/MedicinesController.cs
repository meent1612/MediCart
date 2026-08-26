using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using MediCart.Web.Models;

namespace MediCart.Web.Controllers
{
    public class MedicinesController : Controller
    {
        // NOTE for backend teammates: this static list is a placeholder so the
        // Browse page has real data to render against. Swap GetCatalogue() for
        // a database/service call once the Medicine table + repository exist —
        // the view and JS do not need to change, they just consume this shape.
        public IActionResult Index()
        {
            var medicines = GetCatalogue();
            return View(medicines);
        }

        private static List<MedicineViewModel> GetCatalogue() => new()
        {
            new MedicineViewModel
            {
                Id = 1, Name = "Seclo 20", GenericName = "Omeprazole 20mg", Manufacturer = "Square Pharma",
                ProductType = "Tablet", Category = "Gastric", UseTags = new(){"Acidity"},
                Price = 60, Stock = StockStatus.InStock, Intensity = "Mild", RequiresPrescription = true,
                Description = "A proton pump inhibitor used to reduce stomach acid production. Commonly prescribed for acidity, gastric ulcers, and GERD.",
                DosageInstructions = "1 capsule daily before breakfast, or as directed by your physician.",
                SideEffects = new(){"Headache","Nausea","Stomach pain","Diarrhoea","Dizziness"}
            },
            new MedicineViewModel
            {
                Id = 2, Name = "Maxpro 20", GenericName = "Esomeprazole 20mg", Manufacturer = "Beximco",
                ProductType = "Tablet", Category = "Gastric", UseTags = new(){"Acidity"},
                Price = 70, Stock = StockStatus.InStock, Intensity = "Mild", RequiresPrescription = false,
                Description = "Reduces excess stomach acid to relieve heartburn and acid reflux symptoms.",
                DosageInstructions = "1 tablet daily, preferably in the morning before a meal.",
                SideEffects = new(){"Headache","Flatulence","Nausea","Dry mouth"}
            },
            new MedicineViewModel
            {
                Id = 3, Name = "Sergel 20", GenericName = "Esomeprazole 20mg", Manufacturer = "Healthcare",
                ProductType = "Tablet", Category = "Gastric", UseTags = new(){"Acidity"},
                Price = 65, Stock = StockStatus.InStock, Intensity = "Mild", RequiresPrescription = false,
                Description = "Used for the short-term treatment of acidity, gastric ulcers, and reflux disease.",
                DosageInstructions = "1 tablet once daily, swallowed whole with water.",
                SideEffects = new(){"Headache","Abdominal pain","Constipation","Nausea"}
            },
            new MedicineViewModel
            {
                Id = 4, Name = "Losectil 20", GenericName = "Omeprazole 20mg", Manufacturer = "Incepta",
                ProductType = "Tablet", Category = "Gastric", UseTags = new(){"Acidity"},
                Price = 55, Stock = StockStatus.LowStock, Intensity = "Mild", RequiresPrescription = false,
                Description = "Lowers stomach acid to ease indigestion, heartburn, and peptic ulcer discomfort.",
                DosageInstructions = "1 capsule daily before food unless your doctor advises otherwise.",
                SideEffects = new(){"Nausea","Vomiting","Gas","Stomach pain","Fatigue"}
            },
            new MedicineViewModel
            {
                Id = 5, Name = "Pantonix 40", GenericName = "Pantoprazole 40mg", Manufacturer = "ACI",
                ProductType = "Tablet", Category = "Gastric", UseTags = new(){"Acidity"},
                Price = 80, Stock = StockStatus.InStock, Intensity = "Mild", RequiresPrescription = false,
                Description = "Treats acid-related disorders including gastric and duodenal ulcers.",
                DosageInstructions = "1 tablet daily, at least 1 hour before a meal.",
                SideEffects = new(){"Headache","Diarrhoea","Joint pain","Dizziness"}
            },
            new MedicineViewModel
            {
                Id = 6, Name = "Rabeprazole 20", GenericName = "Rabeprazole 20mg", Manufacturer = "Renata",
                ProductType = "Tablet", Category = "Gastric", UseTags = new(){"Acidity"},
                Price = 72, Stock = StockStatus.InStock, Intensity = "Mild", RequiresPrescription = false,
                Description = "Reduces stomach acid to relieve symptoms of GERD and duodenal ulcers.",
                DosageInstructions = "1 tablet daily in the morning, with or without food.",
                SideEffects = new(){"Headache","Nausea","Rash","Dizziness"}
            },
            new MedicineViewModel
            {
                Id = 7, Name = "Napa Extra", GenericName = "Paracetamol 500mg + Caffeine 65mg", Manufacturer = "Beximco",
                ProductType = "Tablet", Category = "Pain relief", UseTags = new(){"Fever","Headache"},
                Price = 25, Stock = StockStatus.InStock, Intensity = "Mild", RequiresPrescription = false,
                Description = "Fast-acting pain and fever relief, enhanced with caffeine for headaches and migraines.",
                DosageInstructions = "1–2 tablets every 6 hours as needed. Do not exceed 8 tablets in 24 hours.",
                SideEffects = new(){"Nausea","Restlessness","Rare allergic rash"}
            },
            new MedicineViewModel
            {
                Id = 8, Name = "Ace Plus", GenericName = "Paracetamol 500mg", Manufacturer = "Square Pharma",
                ProductType = "Tablet", Category = "Pain relief", UseTags = new(){"Fever","Headache"},
                Price = 15, Stock = StockStatus.InStock, Intensity = "Mild", RequiresPrescription = false,
                Description = "A general-purpose pain reliever and fever reducer suitable for everyday aches.",
                DosageInstructions = "1–2 tablets every 4–6 hours. Maximum 8 tablets per day.",
                SideEffects = new(){"Nausea","Liver strain in overdose","Rare skin rash"}
            },
            new MedicineViewModel
            {
                Id = 9, Name = "Fexo 120", GenericName = "Fexofenadine 120mg", Manufacturer = "ACI",
                ProductType = "Tablet", Category = "Allergy", UseTags = new(){"Fever"},
                Price = 90, Stock = StockStatus.InStock, Intensity = "Mild", RequiresPrescription = false,
                Description = "Non-drowsy antihistamine for seasonal allergies, sneezing, and itchy eyes.",
                DosageInstructions = "1 tablet once daily.",
                SideEffects = new(){"Drowsiness (rare)","Headache","Dry mouth"}
            },
            new MedicineViewModel
            {
                Id = 10, Name = "Tusca Plus", GenericName = "Dextromethorphan Syrup", Manufacturer = "Incepta",
                ProductType = "Syrup", Category = "Fever & cold", UseTags = new(){"Cough"},
                Price = 95, Stock = StockStatus.LowStock, Intensity = "Moderate", RequiresPrescription = false,
                Description = "Suppresses dry, irritating cough and soothes the throat.",
                DosageInstructions = "10ml, 3 times daily after meals. Not for children under 6.",
                SideEffects = new(){"Drowsiness","Dizziness","Nausea"}
            },
            new MedicineViewModel
            {
                Id = 11, Name = "Vitalux Gold", GenericName = "Multivitamin + Multimineral", Manufacturer = "Renata",
                ProductType = "Tablet", Category = "Vitamins", UseTags = new(){"Fever"},
                Price = 350, Stock = StockStatus.InStock, Intensity = "Mild", RequiresPrescription = false,
                Description = "Daily multivitamin supplement to support general health and immunity.",
                DosageInstructions = "1 tablet daily after breakfast.",
                SideEffects = new(){"Mild stomach upset","Nausea if taken empty-stomach"}
            },
            new MedicineViewModel
            {
                Id = 12, Name = "Ketorol Gel", GenericName = "Ketorolac Gel 1%", Manufacturer = "Beximco",
                ProductType = "Ointment", Category = "Pain relief", UseTags = new(){"Headache"},
                Price = 110, Stock = StockStatus.OutOfStock, Intensity = "Moderate", RequiresPrescription = true,
                Description = "Topical anti-inflammatory gel for localized muscle and joint pain.",
                DosageInstructions = "Apply a thin layer to the affected area 3–4 times daily.",
                SideEffects = new(){"Skin irritation","Redness","Burning sensation at application site"}
            }
        };
    }
}

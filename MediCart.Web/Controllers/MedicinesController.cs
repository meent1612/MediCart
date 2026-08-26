using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using MediCart.Web.Models;

namespace MediCart.Web.Controllers
{
    public class MedicinesController : Controller
    {
        public IActionResult Index()
        {
            var medicines = GetSeedData();
            return View(medicines);
        }

        // TODO(backend): replace with data from ApplicationDbContext once the
        // Medicines table / migration is ready. Kept as static seed data so the
        // frontend can be built and demoed independently.
        private static List<MedicineViewModel> GetSeedData()
        {
            return new List<MedicineViewModel>
            {
                new MedicineViewModel
                {
                    Id = 1, Name = "Napa Extra", Composition = "Paracetamol + Caffeine", Strength = "500mg+65mg",
                    Manufacturer = "Beximco Pharma", ProductType = "Tablet", Category = "Pain relief",
                    UseTags = new() { "Headache", "Fever" }, Price = 15, Stock = 240, RequiresRx = false,
                    Potency = "Mild", Popularity = 98,
                    About = "Fast-acting relief for headache, fever, and mild to moderate pain, with added caffeine to boost the effect of paracetamol.",
                    Dosage = "1 tablet every 6 hours after meals. Do not exceed 4 tablets in 24 hours.",
                    SideEffects = new() { "Nausea", "Stomach upset", "Dizziness", "Allergic skin rash (rare)" }
                },
                new MedicineViewModel
                {
                    Id = 2, Name = "Seclo 20", Composition = "Omeprazole", Strength = "20mg",
                    Manufacturer = "Square Pharma", ProductType = "Tablet", Category = "Gastric",
                    UseTags = new() { "Acidity" }, Price = 60, Stock = 150, RequiresRx = true,
                    Potency = "Mild", Popularity = 95,
                    About = "Reduces stomach acid production to relieve heartburn, acid reflux, and gastric ulcers.",
                    Dosage = "1 capsule daily before breakfast, or as prescribed by your physician.",
                    SideEffects = new() { "Headache", "Nausea", "Diarrhea" }
                },
                new MedicineViewModel
                {
                    Id = 3, Name = "Maxpro 20", Composition = "Esomeprazole", Strength = "20mg",
                    Manufacturer = "Beximco", ProductType = "Tablet", Category = "Gastric",
                    UseTags = new() { "Acidity" }, Price = 70, Stock = 120, RequiresRx = false,
                    Potency = "Mild", Popularity = 80,
                    About = "Controls excess stomach acid, easing gastric pain, reflux, and indigestion.",
                    Dosage = "1 tablet daily before breakfast, swallowed whole with water.",
                    SideEffects = new() { "Headache", "Bloating", "Dry mouth" }
                },
                new MedicineViewModel
                {
                    Id = 4, Name = "Sergel 20", Composition = "Esomeprazole", Strength = "20mg",
                    Manufacturer = "Healthcare", ProductType = "Tablet", Category = "Gastric",
                    UseTags = new() { "Acidity" }, Price = 65, Stock = 90, RequiresRx = false,
                    Potency = "Mild", Popularity = 76,
                    About = "Relieves symptoms of acid reflux and helps heal irritation of the esophagus.",
                    Dosage = "1 tablet once daily, at least one hour before a meal.",
                    SideEffects = new() { "Nausea", "Flatulence", "Headache" }
                },
                new MedicineViewModel
                {
                    Id = 5, Name = "Losectil 20", Composition = "Omeprazole", Strength = "20mg",
                    Manufacturer = "Incepta", ProductType = "Tablet", Category = "Gastric",
                    UseTags = new() { "Acidity" }, Price = 55, Stock = 20, RequiresRx = false,
                    Potency = "Mild", Popularity = 60,
                    About = "Short-term treatment for gastric ulcer, reflux disease, and excess acid conditions.",
                    Dosage = "1 capsule daily in the morning, before food.",
                    SideEffects = new() { "Headache", "Abdominal pain", "Constipation" }
                },
                new MedicineViewModel
                {
                    Id = 6, Name = "Pantonix 40", Composition = "Pantoprazole", Strength = "40mg",
                    Manufacturer = "ACI", ProductType = "Tablet", Category = "Gastric",
                    UseTags = new() { "Acidity" }, Price = 80, Stock = 200, RequiresRx = false,
                    Potency = "Mild", Popularity = 70,
                    About = "Treats acid-related damage to the stomach and esophagus lining.",
                    Dosage = "1 tablet daily, swallowed whole, with or without food.",
                    SideEffects = new() { "Diarrhea", "Nausea", "Joint pain" }
                },
                new MedicineViewModel
                {
                    Id = 7, Name = "Rabeprazole 20", Composition = "Rabeprazole", Strength = "20mg",
                    Manufacturer = "Renata", ProductType = "Tablet", Category = "Gastric",
                    UseTags = new() { "Acidity" }, Price = 72, Stock = 110, RequiresRx = false,
                    Potency = "Mild", Popularity = 55,
                    About = "Decreases stomach acid to relieve GERD symptoms and support ulcer healing.",
                    Dosage = "1 tablet daily in the morning before breakfast.",
                    SideEffects = new() { "Headache", "Dizziness", "Rash" }
                },
                new MedicineViewModel
                {
                    Id = 8, Name = "Ace Plus Syrup", Composition = "Paracetamol", Strength = "125mg/5ml",
                    Manufacturer = "Square Pharma", ProductType = "Syrup", Category = "Fever & cold",
                    UseTags = new() { "Fever", "Headache" }, Price = 35, Stock = 80, RequiresRx = false,
                    Potency = "Mild", Popularity = 65,
                    About = "Suitable for children and adults, relieves fever and mild pain in syrup form.",
                    Dosage = "10ml every 6 hours after meals, or as advised by a physician.",
                    SideEffects = new() { "Nausea", "Allergic reaction (rare)" }
                },
                new MedicineViewModel
                {
                    Id = 9, Name = "Adovas Cough Syrup", Composition = "Dextromethorphan", Strength = "10mg/5ml",
                    Manufacturer = "ACME", ProductType = "Syrup", Category = "Fever & cold",
                    UseTags = new() { "Cough" }, Price = 90, Stock = 45, RequiresRx = false,
                    Potency = "Mild", Popularity = 50,
                    About = "Suppresses dry cough and soothes throat irritation.",
                    Dosage = "10ml every 8 hours. Not recommended for children under 6.",
                    SideEffects = new() { "Drowsiness", "Dizziness", "Nausea" }
                },
                new MedicineViewModel
                {
                    Id = 10, Name = "Ceevit Vitamin C", Composition = "Ascorbic acid", Strength = "500mg",
                    Manufacturer = "Square Pharma", ProductType = "Tablet", Category = "Vitamins",
                    UseTags = new(), Price = 40, Stock = 300, RequiresRx = false,
                    Potency = "Mild", Popularity = 40,
                    About = "Supports immune function and antioxidant protection with daily vitamin C.",
                    Dosage = "1 tablet daily after breakfast.",
                    SideEffects = new() { "Stomach upset at high doses" }
                },
                new MedicineViewModel
                {
                    Id = 11, Name = "Fexo 120", Composition = "Fexofenadine", Strength = "120mg",
                    Manufacturer = "Beximco", ProductType = "Tablet", Category = "Allergy",
                    UseTags = new(), Price = 45, Stock = 10, RequiresRx = false,
                    Potency = "Mild", Popularity = 38,
                    About = "Non-drowsy antihistamine for seasonal allergy symptoms and hives.",
                    Dosage = "1 tablet once daily.",
                    SideEffects = new() { "Headache", "Drowsiness (uncommon)" }
                },
                new MedicineViewModel
                {
                    Id = 12, Name = "Voltalin Gel", Composition = "Diclofenac", Strength = "1% w/w",
                    Manufacturer = "ACI", ProductType = "Ointment", Category = "Pain relief",
                    UseTags = new() { "Headache" }, Price = 110, Stock = 0, RequiresRx = false,
                    Potency = "Strong", Popularity = 30,
                    About = "Topical anti-inflammatory gel for muscle and joint pain relief.",
                    Dosage = "Apply a thin layer to the affected area 3-4 times daily.",
                    SideEffects = new() { "Skin irritation", "Redness" }
                },
                new MedicineViewModel
                {
                    Id = 13, Name = "Ketorolac Injection", Composition = "Ketorolac", Strength = "30mg/ml",
                    Manufacturer = "Incepta", ProductType = "Injection", Category = "Pain relief",
                    UseTags = new(), Price = 150, Stock = 60, RequiresRx = true,
                    Potency = "Strong", Popularity = 20,
                    About = "Fast-acting injectable for short-term management of moderate to severe pain.",
                    Dosage = "As administered by a qualified healthcare professional only.",
                    SideEffects = new() { "Injection site pain", "Nausea", "Drowsiness" }
                },
                new MedicineViewModel
                {
                    Id = 14, Name = "Refresh Eye Drops", Composition = "Carboxymethylcellulose", Strength = "0.5%",
                    Manufacturer = "Allergan", ProductType = "Drops", Category = "Allergy",
                    UseTags = new(), Price = 180, Stock = 55, RequiresRx = false,
                    Potency = "Mild", Popularity = 25,
                    About = "Lubricating eye drops that relieve dryness, irritation, and allergy discomfort.",
                    Dosage = "1-2 drops in each eye as needed, up to 4 times daily.",
                    SideEffects = new() { "Temporary blurred vision", "Mild stinging" }
                },
            };
        }
    }
}

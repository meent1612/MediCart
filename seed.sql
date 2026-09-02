-- ============================================================
-- MediCart seed data
-- Run in Neon SQL Editor after dotnet ef database update
-- ============================================================

-- ============================================================
-- 1. Divisions (8 official divisions of Bangladesh)
-- ============================================================

INSERT INTO "Divisions" ("Name", "DeliveryCharge") VALUES
    ('Dhaka',       60.00),
    ('Chattogram',  100.00),
    ('Khulna',      110.00),
    ('Rajshahi',    120.00),
    ('Barishal',    130.00),
    ('Sylhet',      120.00),
    ('Rangpur',     130.00),
    ('Mymensingh',  110.00)
ON CONFLICT DO NOTHING;

-- ============================================================
-- 2. Cities (mapped to DivisionId by name lookup)
-- ============================================================

INSERT INTO "Cities" ("DivisionId", "Name")
SELECT d."Id", c."Name"
FROM (VALUES

    -- Dhaka
    ('Dhaka', 'Adabor'),
    ('Dhaka', 'Badda'),
    ('Dhaka', 'Banani'),
    ('Dhaka', 'Bangshal'),
    ('Dhaka', 'Cantonment'),
    ('Dhaka', 'Chowkbazar'),
    ('Dhaka', 'Dakshinkhan'),
    ('Dhaka', 'Demra'),
    ('Dhaka', 'Dhanmondi'),
    ('Dhaka', 'Gendaria'),
    ('Dhaka', 'Gulshan'),
    ('Dhaka', 'Hazaribagh'),
    ('Dhaka', 'Jatrabari'),
    ('Dhaka', 'Kafrul'),
    ('Dhaka', 'Kalabagan'),
    ('Dhaka', 'Kamrangirchar'),
    ('Dhaka', 'Khilgaon'),
    ('Dhaka', 'Khilkhet'),
    ('Dhaka', 'Kotwali'),
    ('Dhaka', 'Lalbagh'),
    ('Dhaka', 'Mirpur'),
    ('Dhaka', 'Mohammadpur'),
    ('Dhaka', 'Motijheel'),
    ('Dhaka', 'Mugda'),
    ('Dhaka', 'New Market'),
    ('Dhaka', 'Pallabi'),
    ('Dhaka', 'Paltan'),
    ('Dhaka', 'Ramna'),
    ('Dhaka', 'Rayer Bazar'),
    ('Dhaka', 'Sabujbagh'),
    ('Dhaka', 'Shah Ali'),
    ('Dhaka', 'Shahjahanpur'),
    ('Dhaka', 'Sher-e-Bangla Nagar'),
    ('Dhaka', 'Shyampur'),
    ('Dhaka', 'Sutrapur'),
    ('Dhaka', 'Tejgaon'),
    ('Dhaka', 'Turag'),
    ('Dhaka', 'Uttara'),
    ('Dhaka', 'Uttarkhan'),
    ('Dhaka', 'Wari'),

    -- Chattogram
    ('Chattogram', 'Akbar Shah'),
    ('Chattogram', 'Bayazid Bostami'),
    ('Chattogram', 'Bakalia'),
    ('Chattogram', 'Bandar'),
    ('Chattogram', 'Chandgaon'),
    ('Chattogram', 'Chawkbazar'),
    ('Chattogram', 'Double Mooring'),
    ('Chattogram', 'Eid Gah'),
    ('Chattogram', 'Halishahar'),
    ('Chattogram', 'Karnaphuli'),
    ('Chattogram', 'Kotwali'),
    ('Chattogram', 'Pahartali'),
    ('Chattogram', 'Panchlaish'),
    ('Chattogram', 'Patenga'),
    ('Chattogram', 'Raozan'),
    ('Chattogram', 'Sitakunda'),

    -- Khulna
    ('Khulna', 'Daulatpur'),
    ('Khulna', 'Digholia'),
    ('Khulna', 'Dumuria'),
    ('Khulna', 'Khalishpur'),
    ('Khulna', 'Khan Jahan Ali'),
    ('Khulna', 'Khulna Sadar'),
    ('Khulna', 'Labanchara'),
    ('Khulna', 'Rupsa'),
    ('Khulna', 'Sonadanga'),

    -- Rajshahi
    ('Rajshahi', 'Boalia'),
    ('Rajshahi', 'Godagari'),
    ('Rajshahi', 'Matihar'),
    ('Rajshahi', 'Motihar'),
    ('Rajshahi', 'Paba'),
    ('Rajshahi', 'Rajpara'),
    ('Rajshahi', 'Shah Makhdum'),
    ('Rajshahi', 'Tanore'),

    -- Barishal
    ('Barishal', 'Agailjhara'),
    ('Barishal', 'Babuganj'),
    ('Barishal', 'Bakerganj'),
    ('Barishal', 'Band Road'),
    ('Barishal', 'Barisal Sadar'),
    ('Barishal', 'Gournadi'),
    ('Barishal', 'Hizla'),
    ('Barishal', 'Kotwali'),
    ('Barishal', 'Mehendiganj'),
    ('Barishal', 'Muladi'),
    ('Barishal', 'Wazirpur'),

    -- Sylhet
    ('Sylhet', 'Ambarkhana'),
    ('Sylhet', 'Balaganj'),
    ('Sylhet', 'Beanibazar'),
    ('Sylhet', 'Bishwanath'),
    ('Sylhet', 'Companiganj'),
    ('Sylhet', 'Dakshin Surma'),
    ('Sylhet', 'Fenchuganj'),
    ('Sylhet', 'Golapganj'),
    ('Sylhet', 'Jaintiapur'),
    ('Sylhet', 'Kanaighat'),
    ('Sylhet', 'Osmani Nagar'),
    ('Sylhet', 'Sylhet Sadar'),
    ('Sylhet', 'Zindabazar'),

    -- Rangpur
    ('Rangpur', 'Badarganj'),
    ('Rangpur', 'Gangachara'),
    ('Rangpur', 'Kaunia'),
    ('Rangpur', 'Kotwali'),
    ('Rangpur', 'Mahiganj'),
    ('Rangpur', 'Mithapukur'),
    ('Rangpur', 'Pirgacha'),
    ('Rangpur', 'Pirganj'),
    ('Rangpur', 'Rangpur Sadar'),
    ('Rangpur', 'Taraganj'),

    -- Mymensingh
    ('Mymensingh', 'Bhaluka'),
    ('Mymensingh', 'Dhobaura'),
    ('Mymensingh', 'Fulbaria'),
    ('Mymensingh', 'Gaffargaon'),
    ('Mymensingh', 'Gauripur'),
    ('Mymensingh', 'Haluaghat'),
    ('Mymensingh', 'Ishwarganj'),
    ('Mymensingh', 'Kotwali'),
    ('Mymensingh', 'Muktagachha'),
    ('Mymensingh', 'Mymensingh Sadar'),
    ('Mymensingh', 'Nandail'),
    ('Mymensingh', 'Phulpur'),
    ('Mymensingh', 'Trishal')

) AS c("DivisionName", "Name")
JOIN "Divisions" d ON d."Name" = c."DivisionName"
ON CONFLICT DO NOTHING;

-- ============================================================
-- Top-level Categories
-- ============================================================

INSERT INTO "Categories" ("Name", "Description", "ParentCategoryId", "CreatedAt")
SELECT v."Name", v."Description", NULL, NOW()
FROM (VALUES
    ('Medicine',                 'Prescription and OTC drugs organised by condition'),
    ('Vitamins & Supplements',   'Vitamins, minerals and dietary supplements'),
    ('Diabetic Care',            'Diabetes management supplies and devices'),
    ('Women''s Care',            'Feminine health, mother care, and hygiene products')
) AS v("Name", "Description")
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c WHERE c."Name" = v."Name" AND c."ParentCategoryId" IS NULL
);

-- ============================================================
-- Subcategories
-- ============================================================

INSERT INTO "Categories" ("Name", "Description", "ParentCategoryId", "CreatedAt")
SELECT sc."Name", NULL, parent."Id", NOW()
FROM (VALUES
    -- Medicine
    ('Medicine', 'Allergies & Asthma'),
    ('Medicine', 'Epilepsy & Neurological'),
    ('Medicine', 'Pain Relief (Analgesics)'),
    ('Medicine', 'Gastrointestinal'),
    ('Medicine', 'Antibiotics & Anti-infectives'),
    ('Medicine', 'Cardiac & Blood Pressure'),
    ('Medicine', 'Diabetes'),
    ('Medicine', 'Hormonal & Endocrine'),
    ('Medicine', 'Mental Health'),

    -- Vitamins & Supplements
    ('Vitamins & Supplements', 'Multivitamins'),
    ('Vitamins & Supplements', 'Vitamin D'),
    ('Vitamins & Supplements', 'Vitamin C & Antioxidants'),
    ('Vitamins & Supplements', 'Calcium & Bone Health'),
    ('Vitamins & Supplements', 'Herbal & Natural Supplements'),
    ('Vitamins & Supplements', 'Protein & Fitness Supplements'),
    ('Vitamins & Supplements', 'Omega-3 & Fish Oil'),

    -- Diabetic Care
    ('Diabetic Care', 'Blood Glucose Monitors (Glucometers)'),
    ('Diabetic Care', 'Test Strips & Lancets'),
    ('Diabetic Care', 'Insulin Pens & Syringes'),
    ('Diabetic Care', 'Diabetic Care Kits'),

    -- Women's Care
    ('Women''s Care', 'Feminine Hygiene'),
    ('Women''s Care', 'Mother Care (Prenatal & Postnatal)'),
    ('Women''s Care', 'Women''s Health Medications')
) AS sc("ParentName", "Name")
JOIN "Categories" parent ON parent."Name" = sc."ParentName" AND parent."ParentCategoryId" IS NULL
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c WHERE c."Name" = sc."Name" AND c."ParentCategoryId" = parent."Id"
);

-- ============================================================
-- Product Types
-- ============================================================

INSERT INTO "ProductTypes" ("Name")
SELECT v."Name"
FROM (VALUES
    ('Tablet / Caplet'),
    ('Capsule'),
    ('Syrup / Suspension'),
    ('Injection'),
    ('Drops (Pediatric/Children''s)'),
    ('Cream / Ointment'),
    ('Powder')
) AS v("Name")
WHERE NOT EXISTS (
    SELECT 1 FROM "ProductTypes" p WHERE p."Name" = v."Name"
);
ON CONFLICT DO NOTHING;
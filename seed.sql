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

INSERT INTO "Categories" ("Name", "Description", "CreatedAt")
SELECT v."Name", v."Description", NOW()
FROM (VALUES
    ('Medicine',                 'Prescription and OTC drugs organised by condition'),
    ('Vitamins & Supplements',   'Vitamins, minerals and dietary supplements'),
    ('Diabetic Care',            'Diabetes management supplies and devices'),
    ('Women''s Care',            'Feminine health, mother care, and hygiene products')
) AS v("Name", "Description")
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c WHERE c."Name" = v."Name"
);

-- ============================================================
-- SubCategories
-- ============================================================

INSERT INTO "SubCategories" ("CategoryId", "Name", "CreatedAt")
SELECT parent."Id", sc."Name", NOW()
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
JOIN "Categories" parent ON parent."Name" = sc."ParentName"
WHERE NOT EXISTS (
    SELECT 1 FROM "SubCategories" existing
    WHERE existing."Name" = sc."Name" AND existing."CategoryId" = parent."Id"
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
-- ============================================================
-- 4. Medicines (test data — 8 medicines, ImageUrl left NULL for UI upload)
-- ============================================================

INSERT INTO "Medicines" (
    "CategoryId", "SubCategoryId", "ProductTypeId",
    "Name", "GenericName", "Manufacturer", "Price", "Unit",
    "Description", "Dosage", "RequiresPrescription", "SensitivityLevel",
    "ImageUrl", "CreatedAt"
)
SELECT
    cat."Id", sub."Id", pt."Id",
    m."Name", m."GenericName", m."Manufacturer", m."Price", m."Unit",
    m."Description", m."Dosage", m."RequiresPrescription", m."SensitivityLevel",
    NULL, NOW()
FROM (VALUES
    ('Arlin 600',      'Ibuprofen',                        'Beacon Pharmaceuticals',     8.00,  '1 strip of 10 tablets',  'Nonsteroidal anti-inflammatory drug (NSAID) used for pain, inflammation, and fever.',                              'Adults: 1 tablet every 6-8 hours after meals, not exceeding 3 tablets/day unless advised by a physician.', false, null,  'Medicine',               'Pain Relief (Analgesics)',       'Tablet / Caplet'),
    ('Augment 500',    'Amoxicillin + Clavulanic Acid',    'GlaxoSmithKline Bangladesh', 45.00, '1 strip of 10 tablets',  'Broad-spectrum antibiotic combination used to treat bacterial infections of the respiratory tract, ear, and skin.', 'Adults: 1 tablet every 12 hours for 5-7 days, or as prescribed by a physician. Complete the full course.',  true,  'mid', 'Medicine',               'Antibiotics & Anti-infectives',  'Tablet / Caplet'),
    ('Basak',          'Adhatoda Vasica (Vasaka) Extract', 'Square Pharmaceuticals',     55.00, '1 bottle of 100ml',      'Herbal expectorant syrup used to relieve cough, cold, and bronchial congestion.',                                   'Adults: 2 teaspoons (10ml) 3 times daily. Children: 1 teaspoon (5ml) 3 times daily, or as advised.',        false, null,  'Vitamins & Supplements','Herbal & Natural Supplements',   'Syrup / Suspension'),
    ('Fexo 120',       'Fexofenadine Hydrochloride',       'Square Pharmaceuticals',     6.50,  '1 strip of 10 tablets',  'Antihistamine used to relieve allergy symptoms such as sneezing, runny nose, and itchy eyes.',                     'Adults and children over 12: 1 tablet once daily.',                                                          false, null,  'Medicine',               'Allergies & Asthma',             'Tablet / Caplet'),
    ('Multivit Plus',  'Multivitamins & Minerals',         'ACI Limited',                4.50,  '1 strip of 10 tablets',  'Daily multivitamin and mineral supplement to support general health and immunity.',                               'Adults: 1 tablet daily after breakfast, or as advised by a physician.',                                      false, null,  'Vitamins & Supplements','Multivitamins',                  'Tablet / Caplet'),
    ('Napa Extra',     'Paracetamol + Caffeine',           'Beximco Pharmaceuticals',    2.00,  '1 strip of 12 tablets',  'Fast-acting pain reliever and fever reducer with added caffeine for enhanced effect.',                             'Adults: 1-2 tablets every 4-6 hours as needed, not exceeding 8 tablets in 24 hours.',                        false, null,  'Medicine',               'Pain Relief (Analgesics)',       'Tablet / Caplet'),
    ('Nexum 40',       'Esomeprazole',                     'Beximco Pharmaceuticals',    12.00, '1 strip of 14 capsules', 'Proton pump inhibitor used to treat acid reflux, heartburn, and stomach ulcers.',                                  'Adults: 1 capsule once daily before breakfast, for 4-8 weeks or as prescribed.',                             true,  null,  'Medicine',               'Gastrointestinal',               'Capsule'),
    ('Lanso D',        'Lansoprazole + Domperidone',       'ACME Laboratories',          15.00, '1 strip of 10 capsules', 'Combination capsule used to treat acid reflux and associated nausea or bloating.',                                'Adults: 1 capsule once daily before breakfast, or as advised by a physician.',                               true,  null,  'Medicine',               'Gastrointestinal',               'Capsule')
) AS m(
    "Name", "GenericName", "Manufacturer", "Price", "Unit",
    "Description", "Dosage", "RequiresPrescription", "SensitivityLevel",
    "CategoryName", "SubCategoryName", "ProductTypeName"
)
JOIN "Categories" cat ON cat."Name" = m."CategoryName"
JOIN "SubCategories" sub ON sub."Name" = m."SubCategoryName" AND sub."CategoryId" = cat."Id"
JOIN "ProductTypes" pt ON pt."Name" = m."ProductTypeName"
WHERE NOT EXISTS (
    SELECT 1 FROM "Medicines" existing WHERE existing."Name" = m."Name"
);

-- ============================================================
-- 5. Stock records for the above medicines
-- ============================================================

INSERT INTO "Stocks" ("MedicineId", "Quantity", "ExpiryDate", "UpdatedAt")
SELECT med."Id", s."Quantity", s."ExpiryDate"::date, NOW()
FROM (VALUES
    ('Arlin 600',     40,  '2027-06-30'),
    ('Augment 500',   25,  '2027-03-31'),
    ('Basak',         60,  '2027-09-30'),
    ('Fexo 120',      80,  '2027-12-31'),
    ('Multivit Plus', 100, '2028-01-31'),
    ('Napa Extra',    120, '2027-08-31'),
    ('Nexum 40',      35,  '2027-05-31'),
    ('Lanso D',       30,  '2027-04-30')
) AS s("MedicineName", "Quantity", "ExpiryDate")
JOIN "Medicines" med ON med."Name" = s."MedicineName"
ON CONFLICT ("MedicineId") DO NOTHING;

-- ============================================================
-- 6. Side effects for the above medicines
-- ============================================================

INSERT INTO "SideEffects" ("MedicineId", "Effect", "Severity")
SELECT med."Id", se."Effect", se."Severity"
FROM (VALUES
    ('Arlin 600',     'Stomach upset',                       'mild'),
    ('Arlin 600',     'Heartburn',                            'mild'),
    ('Augment 500',   'Diarrhea',                              'moderate'),
    ('Augment 500',   'Nausea',                                'mild'),
    ('Basak',         'Mild drowsiness',                       'mild'),
    ('Fexo 120',      'Headache',                              'mild'),
    ('Fexo 120',      'Dry mouth',                             'mild'),
    ('Multivit Plus', 'Mild nausea if taken on empty stomach', 'mild'),
    ('Napa Extra',    'Nausea',                                'mild'),
    ('Napa Extra',    'Insomnia (due to caffeine)',            'mild'),
    ('Nexum 40',      'Headache',                              'mild'),
    ('Nexum 40',      'Abdominal pain',                        'moderate'),
    ('Lanso D',       'Dry mouth',                             'mild'),
    ('Lanso D',       'Dizziness',                             'moderate')
) AS se("MedicineName", "Effect", "Severity")
JOIN "Medicines" med ON med."Name" = se."MedicineName"
WHERE NOT EXISTS (
    SELECT 1 FROM "SideEffects" existing
    WHERE existing."MedicineId" = med."Id" AND existing."Effect" = se."Effect"
);
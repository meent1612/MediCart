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

    -- Medicine (9 — untouched, already in DB)
    ('Medicine', 'Allergies & Asthma'),
    ('Medicine', 'Epilepsy & Neurological'),
    ('Medicine', 'Pain Relief (Analgesics)'),
    ('Medicine', 'Gastrointestinal'),
    ('Medicine', 'Antibiotics & Anti-infectives'),
    ('Medicine', 'Cardiac & Blood Pressure'),
    ('Medicine', 'Diabetes'),
    ('Medicine', 'Hormonal & Endocrine'),
    ('Medicine', 'Mental Health'),

    -- Vitamins & Supplements (5)
    ('Vitamins & Supplements', 'Multivitamin'),
    ('Vitamins & Supplements', 'Vitamins and Minerals'),
    ('Vitamins & Supplements', 'Food Supplement'),
    ('Vitamins & Supplements', 'Herbal'),
    ('Vitamins & Supplements', 'Protein Powder'),

    -- Diabetic Care (6)
    ('Diabetic Care', 'Glucose Meter'),
    ('Diabetic Care', 'Glucose Test Strips'),
    ('Diabetic Care', 'Insulin Cartridge'),
    ('Diabetic Care', 'Insulin Pen (Onetime)'),
    ('Diabetic Care', 'Lancets'),
    ('Diabetic Care', 'Diabetes Medicines'),

    -- Women's Care (4)
    ('Women''s Care', 'Sanitary Pad'),
    ('Women''s Care', 'Birth Control Pill'),
    ('Women''s Care', 'Pregnancy Test'),
    ('Women''s Care', 'Beauty Care')

) AS sc("ParentName", "Name")
JOIN "Categories" parent ON parent."Name" = sc."ParentName"
WHERE NOT EXISTS (
    SELECT 1 FROM "SubCategories" existing
    WHERE existing."Name" = sc."Name"
      AND existing."CategoryId" = parent."Id"
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
    ('Injection / Vial'),
    ('Drops'),
    ('Cream / Ointment'),
    ('Powder'),
    ('Softgel'),
    ('Pen / Device'),
    ('Strip / Test Kit'),
    ('Sanitary Pad')
) AS v("Name")
WHERE NOT EXISTS (
    SELECT 1 FROM "ProductTypes" p WHERE p."Name" = v."Name"
);

-- ============================================================
-- PART 3: Medicines, Stocks, SideEffects
-- 24 subcategories × 2 products = 48 medicines total
-- Every demo scenario covered — see scenario map below
--
-- STOCK SCENARIOS:
--   Normal (>10, expiry >30 days)        : most medicines
--   Low stock (1-10 units)               : Fexo 120, Basak Syrup, NovoMix 30 Penfill,
--                                           Marvelon, Powerlift Weight Gainer,
--                                           Trulicity 0.75mg, Femicon
--   Out of stock (0 units)               : Gaba 300mg, Cloma 2, Freedom Heavy Flow Wings 16pads
--   Warning expiry (8-30 days from now)  : Napa Extra, Metform 500mg
--   Critical expiry (0-7 days) BLOCKED   : Augment 500, Freedom Heavy Flow Wings 16pads
--   Expired (past date) BLOCKED          : Cloma 2 Tablet
--
-- PRESCRIPTION SCENARIOS:
--   RequiresPrescription = true          : 22 medicines (marked Rx below)
--   RequiresPrescription = false         : 26 medicines
--
-- SENSITIVITY / AUTO-FLAG SCENARIOS:
--   high (flag >=5 units)  : Gaba 300mg, Cloma 2
--   mid  (flag >=15 units) : Augment 500, Cazep 200mg
--   low  (flag >=30 units) : Metform 500mg, Glucophage 500mg
--
-- SIDE EFFECT SCENARIOS:
--   mild only     : several OTC products
--   moderate      : antibiotics, cardiac
--   severe        : epilepsy, mental health, insulin
--   mixed         : most Rx medicines
--   no side effects: devices, test kits, pads
-- ============================================================

INSERT INTO "Medicines" (
    "CategoryId", "SubCategoryId", "ProductTypeId",
    "Name", "GenericName", "Manufacturer", "Price", "Unit",
    "Description", "Dosage",
    "RequiresPrescription", "SensitivityLevel",
    "ImageUrl", "CreatedAt"
)
SELECT
    cat."Id", sub."Id", pt."Id",
    m."Name", m."GenericName", m."Manufacturer",
    m."Price"::numeric(10,2), m."Unit",
    m."Description", m."Dosage",
    m."RequiresPrescription"::boolean,
    m."SensitivityLevel",
    m."ImageUrl",
    NOW()
FROM (VALUES

    -- ==============================================================
    -- CATEGORY: Medicine
    -- ==============================================================

    -- SubCat: Allergies & Asthma (2)
    ('Fexo 120',
     'Fexofenadine Hydrochloride 120mg',
     'Square Pharmaceuticals Ltd.',
     '6.50', '1 strip of 10 tablets',
     'Antihistamine tablet for seasonal allergies, hay fever, and allergic rhinitis.',
     'Adults: 1 tablet once daily. Not for children under 12.',
     'false', null,
     'https://res.cloudinary.com/tjgtsydz/image/upload/v1790409430/medicart/medicines/fexo_120_taotb3.jpg',
     'Medicine', 'Allergies & Asthma', 'Tablet / Caplet'),

    ('Montela 10mg',
     'Montelukast Sodium 10mg',
     'Square Pharmaceuticals Ltd.',
     '8.00', '1 strip of 10 tablets',
     'Leukotriene receptor antagonist for chronic asthma and allergic rhinitis.',
     'Adults 15+: 1 tablet (10mg) once daily in the evening.',
     'false', null,
     'https://res.cloudinary.com/tjgtsydz/image/upload/v1790409561/medicart/medicines/montela10_j7tgle.webp',
     'Medicine', 'Allergies & Asthma', 'Tablet / Caplet'),

    -- SubCat: Epilepsy & Neurological (2) — sensitivity: high (flag >=5), mid (flag >=15)
    ('Gaba 300mg',
     'Gabapentin 300mg',
     'Renata Limited',
     '38.00', '1 strip of 10 capsules',
     'Anticonvulsant for epilepsy and neuropathic pain.',
     'Adults: 300mg 3 times daily, adjusted by physician. Do not stop abruptly.',
     'true', 'high',
     'https://res.cloudinary.com/tjgtsydz/image/upload/v1790409871/medicart/medicines/gaba300_onkzmq.jpg',
     'Medicine', 'Epilepsy & Neurological', 'Capsule'),

    ('Cazep 200mg',
     'Carbamazepine 200mg',
     'Beximco Pharmaceuticals Ltd.',
     '39.00', '1 strip of 10 tablets',
     'Anticonvulsant for epilepsy, trigeminal neuralgia, and bipolar disorder.',
     'Adults: 200mg twice daily initially; dose adjusted by physician.',
     'true', 'mid',
     'https://res.cloudinary.com/tjgtsydz/image/upload/v1790409659/medicart/medicines/cazep200_lv82aw.jpg',
     'Medicine', 'Epilepsy & Neurological', 'Tablet / Caplet'),

    -- SubCat: Pain Relief (Analgesics) (2)
    ('Napa Extra',
     'Paracetamol 500mg + Caffeine 65mg',
     'Beximco Pharmaceuticals Ltd.',
     '2.00', '1 strip of 12 tablets',
     'Fast-acting pain reliever and fever reducer with caffeine for enhanced effect.',
     'Adults: 1-2 tablets every 4-6 hours. Max 8 tablets in 24 hours.',
     'false', null,
     'https://res.cloudinary.com/tjgtsydz/image/upload/v1790410114/medicart/medicines/napaextra_g0iwvb.jpg',
     'Medicine', 'Pain Relief (Analgesics)', 'Tablet / Caplet'),

    ('Arlin 600',
     'Ibuprofen 600mg',
     'Beacon Pharmaceuticals Ltd.',
     '8.00', '1 strip of 10 tablets',
     'NSAID for pain, fever, and inflammation including headache and muscle pain.',
     'Adults: 1 tablet every 6-8 hours after meals. Max 3 tablets/day.',
     'false', null,
     'https://res.cloudinary.com/tjgtsydz/image/upload/v1790410138/medicart/medicines/arlin600_rz7y8g.jpg',
     'Medicine', 'Pain Relief (Analgesics)', 'Tablet / Caplet'),

    -- SubCat: Gastrointestinal (2)
    ('Nexum 40',
     'Esomeprazole 40mg',
     'Beximco Pharmaceuticals Ltd.',
     '12.00', '1 strip of 14 capsules',
     'Proton pump inhibitor for acid reflux, GERD, and stomach ulcers.',
     'Adults: 1 capsule daily before breakfast for 4-8 weeks or as directed.',
     'true', null,
     'https://res.cloudinary.com/tjgtsydz/image/upload/v1790410341/medicart/medicines/nexum40_i7rany.webp',
     'Medicine', 'Gastrointestinal', 'Capsule'),

    ('Lanso D',
     'Lansoprazole 30mg + Domperidone 10mg',
     'ACME Laboratories Ltd.',
     '15.00', '1 strip of 10 capsules',
     'Combination for acid reflux with nausea or bloating.',
     'Adults: 1 capsule once daily before breakfast.',
     'true', null,
     'https://res.cloudinary.com/tjgtsydz/image/upload/v1790410367/medicart/medicines/lanso_d_ubapoy.jpg',
     'Medicine', 'Gastrointestinal', 'Capsule'),

    -- SubCat: Antibiotics & Anti-infectives (2) — sensitivity: mid (flag >=15)
    ('Augment 500',
     'Amoxicillin 500mg + Clavulanic Acid 125mg',
     'GlaxoSmithKline Bangladesh Ltd.',
     '45.00', '1 strip of 10 tablets',
     'Broad-spectrum antibiotic for respiratory, ear, and skin infections.',
     'Adults: 1 tablet every 12 hours for 5-7 days. Complete the full course.',
     'true', 'mid',
     'https://res.cloudinary.com/tjgtsydz/image/upload/v1788420004/medicart/medicines/augment_msiopf.jpg',
     'Medicine', 'Antibiotics & Anti-infectives', 'Tablet / Caplet'),

    ('Zimax 500',
     'Azithromycin 500mg',
     'Square Pharmaceuticals Ltd.',
     '55.00', '1 strip of 3 tablets',
     'Macrolide antibiotic for respiratory tract and skin infections.',
     'Adults: 500mg once daily for 3 days, or as prescribed.',
     'true', null,
     'https://res.cloudinary.com/tjgtsydz/image/upload/v1790410916/medicart/medicines/zimax500_pk1q5v.jpg',
     'Medicine', 'Antibiotics & Anti-infectives', 'Tablet / Caplet'),

    -- SubCat: Cardiac & Blood Pressure (2)
    ('Bislol 5mg',
     'Bisoprolol Fumarate 5mg',
     'Square Pharmaceuticals Ltd.',
     '6.00', '1 strip of 14 tablets',
     'Beta-blocker for hypertension, angina, and heart failure management.',
     'Adults: 5mg once daily in the morning; dose adjusted by physician.',
     'true', null,
     'https://res.cloudinary.com/tjgtsydz/image/upload/v1790410974/medicart/medicines/bislol5_tjva48.webp',
     'Medicine', 'Cardiac & Blood Pressure', 'Tablet / Caplet'),

    ('Amlor 5mg',
     'Amlodipine Besylate 5mg',
     'Pfizer Bangladesh Ltd.',
     '5.00', '1 strip of 10 tablets',
     'Calcium channel blocker for hypertension and chronic stable angina.',
     'Adults: 5mg once daily. May be increased to 10mg based on response.',
     'true', null,
     'https://res.cloudinary.com/tjgtsydz/image/upload/v1790411040/medicart/medicines/amlor5_xfckuy.jpg',
     'Medicine', 'Cardiac & Blood Pressure', 'Tablet / Caplet'),

    -- SubCat: Diabetes (Medicine category) (2) — sensitivity: low (flag >=30)
    ('Metform 500mg',
     'Metformin Hydrochloride 500mg',
     'Opsonin Pharma Ltd.',
     '4.00', '1 strip of 10 tablets',
     'First-line oral antidiabetic for Type 2 diabetes management.',
     'Adults: 1 tablet twice daily with meals. Dose adjusted by physician.',
     'true', 'low',
     'https://res.cloudinary.com/tjgtsydz/image/upload/v1790411134/medicart/medicines/metform500_xzmcop.jpg',
     'Medicine', 'Diabetes', 'Tablet / Caplet'),

    ('Glucophage 500mg',
     'Metformin Hydrochloride 500mg',
     'Merck (Bangladesh) Ltd.',
     '29.00', '1 strip of 10 tablets',
     'Brand-name metformin for blood glucose control in Type 2 diabetes.',
     'Adults: 1 tablet 2-3 times daily with meals as prescribed.',
     'true', 'low',
     'https://res.cloudinary.com/tjgtsydz/image/upload/v1790411155/medicart/medicines/glucophage500_wspuvt.jpg',
     'Medicine', 'Diabetes', 'Tablet / Caplet'),

    -- SubCat: Hormonal & Endocrine (2)
    ('Thyronorm 50mcg',
     'Levothyroxine Sodium 50mcg',
     'Abbott Healthcare',
     '12.00', '1 strip of 28 tablets',
     'Thyroid hormone replacement for hypothyroidism.',
     'Adults: As prescribed. Taken on empty stomach 30 min before breakfast.',
     'true', null,
     'https://res.cloudinary.com/tjgtsydz/image/upload/v1790411250/medicart/medicines/thyronorm500_aa1jyr.jpg',
     'Medicine', 'Hormonal & Endocrine', 'Tablet / Caplet'),

    ('Cloma 2',
     'Clomiphene Citrate 2mg',
     'Square Pharmaceuticals Ltd.',
     '100.00', '1 strip of 10 tablets',
     'Ovulation stimulant used in female infertility treatment.',
     'As prescribed by physician. Not for self-medication.',
     'true', 'high',
     'https://res.cloudinary.com/tjgtsydz/image/upload/v1790411292/medicart/medicines/cloma2_sfhytn.webp',
     'Medicine', 'Hormonal & Endocrine', 'Tablet / Caplet'),

    -- SubCat: Mental Health (2)
    ('Serenace 5mg',
     'Haloperidol 5mg',
     'Drug International Ltd.',
     '5.00', '1 strip of 10 tablets',
     'Antipsychotic for schizophrenia, acute psychosis, and severe agitation.',
     'Adults: 5-10mg/day in divided doses as prescribed by psychiatrist.',
     'true', null,
     'https://res.cloudinary.com/tjgtsydz/image/upload/v1790411447/medicart/medicines/serenace5_v1uncf.jpg',
     'Medicine', 'Mental Health', 'Tablet / Caplet'),

    ('Flunil 20mg',
     'Fluoxetine Hydrochloride 20mg',
     'Incepta Pharmaceuticals Ltd.',
     '6.00', '1 strip of 10 capsules',
     'SSRI antidepressant for depression, OCD, and panic disorder.',
     'Adults: 20mg once daily in the morning. Dose adjusted after 4 weeks.',
     'true', null,
     'https://res.cloudinary.com/tjgtsydz/image/upload/v1790411364/medicart/medicines/flunil20_wiz3l1.jpg',
     'Medicine', 'Mental Health', 'Capsule'),

    -- ==============================================================
    -- CATEGORY: Vitamins & Supplements
    -- ==============================================================

    -- SubCat: Multivitamin (2)
    ('Supravit-S',
     'Multivitamin + Multimineral',
     'Drug International Ltd.',
     '70.00', '1 pot of 25 capsules',
     'Complete daily multivitamin and mineral supplement for general health.',
     'Adults: 1 capsule daily after breakfast.',
     'false', null,
     'https://res.cloudinary.com/tjgtsydz/image/upload/v1790411975/medicart/medicines/supravits_lvoh4d.jpg',
     'Vitamins & Supplements', 'Multivitamin', 'Capsule'),

    ('Revital Woman',
     'Multivitamin with Ginseng',
     'Ranbaxy Laboratories',
     '1499.00', '1 box of 30 tablets',
     'Daily multivitamin with ginseng for women — energy, immunity, and skin health.',
     'Adults: 1 tablet daily after a meal.',
     'false', null,
     'https://res.cloudinary.com/tjgtsydz/image/upload/v1790412009/medicart/medicines/revital_t9fpcn.webp',
     'Vitamins & Supplements', 'Multivitamin', 'Tablet / Caplet'),

    -- SubCat: Vitamins and Minerals (2)
    ('Caltrate 600+D',
     'Calcium Carbonate 600mg + Vitamin D3 400IU',
     'Wyeth Bangladesh Ltd.',
     '2699.00', '1 box of 120 tablets',
     'Calcium and Vitamin D supplement for bone health and calcium deficiency.',
     'Adults: 1 tablet twice daily with meals.',
     'false', null,
     'https://res.cloudinary.com/tjgtsydz/image/upload/v1790412047/medicart/medicines/Caltrate600_D_epir6o.jpg',
     'Vitamins & Supplements', 'Vitamins and Minerals', 'Tablet / Caplet'),

    ('Biovit B Complex',
     'Vitamin B Complex (B1, B2, B6, B12)',
     'ACI Limited',
     '10.00', '1 strip of 10 capsules',
     'B-complex vitamins for nerve health, energy metabolism, and immunity.',
     'Adults: 1 capsule daily after meals.',
     'false', null,
     'https://res.cloudinary.com/tjgtsydz/image/upload/v1790412120/medicart/medicines/Biovit_B_Complex_ouxcyf.jpg',
     'Vitamins & Supplements', 'Vitamins and Minerals', 'Capsule'),

    -- SubCat: Food Supplement (2)
    ('NOW Omega-3 1000mg',
     'Fish Oil 1000mg (Omega-3)',
     'NOW Foods USA',
     '2699.00', '1 box of 100 softgels',
     'Omega-3 fish oil supplement for heart, brain, and joint health.',
     'Adults: 1-2 softgels daily with meals.',
     'false', null,
     'https://res.cloudinary.com/tjgtsydz/image/upload/v1790412177/medicart/medicines/NOW_Omega-3_gadtvu.jpg',
     'Vitamins & Supplements', 'Food Supplement', 'Softgel'),

    ('Truemed Turmeric',
     'Turmeric Curcumin 500mg',
     'Truemed USA',
     '1424.00', '1 box of 60 capsules',
     'Anti-inflammatory turmeric and curcumin supplement for joint and digestive health.',
     'Adults: 1-2 capsules daily with meals.',
     'false', null,
     'https://res.cloudinary.com/tjgtsydz/image/upload/v1790412221/medicart/medicines/Truemed_Turmeric_zvro45.jpg',
     'Vitamins & Supplements', 'Food Supplement', 'Capsule'),

    -- SubCat: Herbal (2)
    ('Basak Syrup',
     'Adhatoda Vasica (Vasaka) Extract',
     'Square Pharmaceuticals Ltd.',
     '55.00', '1 bottle of 100ml',
     'Herbal expectorant syrup for cough, cold, and bronchial congestion.',
     'Adults: 2 teaspoons 3 times daily. Children: 1 teaspoon 3 times daily.',
     'false', null,
     'https://res.cloudinary.com/tjgtsydz/image/upload/v1790412295/medicart/medicines/Basak_Syrup_dnnqsh.png',
     'Vitamins & Supplements', 'Herbal', 'Syrup / Suspension'),

    ('Gintex 500mg',
     'Ginkgo Biloba Extract 500mg',
     'Drug International Ltd.',
     '57.00', '1 strip of 10 capsules',
     'Herbal supplement for memory, concentration, and cerebral circulation.',
     'Adults: 1 capsule twice daily with meals.',
     'false', null,
     'https://res.cloudinary.com/tjgtsydz/image/upload/v1790412332/medicart/medicines/Gintex500_tqj3rt.jpg',
     'Vitamins & Supplements', 'Herbal', 'Capsule'),

    -- SubCat: Protein Powder (2)
    ('Protinex Chocolate',
     'High Protein Nutritional Supplement',
     'Danone India',
     '2549.00', '1 tin of 400g',
     'High-protein chocolate drink mix for adults — muscle health and energy.',
     'Adults: Mix 2 heaped scoops in 200ml milk or water, twice daily.',
     'false', null,
     'https://res.cloudinary.com/tjgtsydz/image/upload/v1790412395/medicart/medicines/Protinex_Chocolate_myu0ma.jpg',
     'Vitamins & Supplements', 'Protein Powder', 'Powder'),

    ('Powerlift Weight Gainer',
     'Whey Protein + Multivitamin Complex',
     'Powerlift Nutrition',
     '1800.00', '1 pouch of 500g',
     'Mass gainer protein powder with vitamins and DigeZyme for muscle building.',
     'Adults: Mix 2-3 scoops in 300ml water or milk post-workout.',
     'false', null,
     'https://res.cloudinary.com/tjgtsydz/image/upload/v1790412431/medicart/medicines/Powerlift_Weight_Gainer_uyxrs3.jpg',
     'Vitamins & Supplements', 'Protein Powder', 'Powder'),

    -- ==============================================================
    -- CATEGORY: Diabetic Care
    -- ==============================================================

    -- SubCat: Glucose Meter (2)
    ('Accu-Chek Active',
     'Blood Glucose Monitor Kit',
     'Roche Diabetes Care',
     '3500.00', '1 kit (meter + 10 strips + lancets)',
     'Fast and accurate blood glucose monitor — results in 5 seconds. Stores 500 readings.',
     'Insert strip, prick fingertip, apply blood to strip. Read result in 5 seconds.',
     'false', null,
     'https://res.cloudinary.com/tjgtsydz/image/upload/v1790412843/medicart/medicines/Egvh0JIAL6wPFlf5RgNZ9L1bu4t2clQcl2dexgnY_z59efz.jpg',
     'Diabetic Care', 'Glucose Meter', 'Pen / Device'),

    ('GlucoSure Star',
     'Blood Glucose Monitoring System',
     'GlucoSure',
     '1800.00', '1 set (meter + accessories)',
     'Affordable blood glucose monitoring machine set for home diabetes management.',
     'Insert strip, prick fingertip, apply blood sample, read result in 8 seconds.',
     'false', null,
     'https://res.cloudinary.com/tjgtsydz/image/upload/v1790412884/medicart/medicines/main_1_uhskwm.jpg',
     'Diabetic Care', 'Glucose Meter', 'Pen / Device'),

    -- SubCat: Glucose Test Strips (2)
    ('Accu-Chek Strips 50pcs',
     'Blood Glucose Test Strips for Accu-Chek Performa',
     'Roche Bangladesh Ltd.',
     '1150.00', '1 pack of 50 strips',
     'Compatible with Accu-Chek Performa glucometers. Fast absorption, no coding required.',
     'Insert strip into Accu-Chek Performa meter. Apply fingertip blood to strip.',
     'false', null,
     'https://res.cloudinary.com/tjgtsydz/image/upload/v1790412958/medicart/medicines/bt37PLqkmt1aOa2vdhYG8R5k5USfnYA7X1gC16il_kefgyh.jpg',
     'Diabetic Care', 'Glucose Test Strips', 'Strip / Test Kit'),

    ('OneTouch Verio Strips 50pcs',
     'Blood Glucose Test Strips for OneTouch Verio',
     'LifeScan (Johnson & Johnson)',
     '2001.00', '1 pack of 50 strips',
     'Precision strips for OneTouch Verio glucometers. Color-coded range indicator.',
     'Insert into OneTouch Verio meter. Apply blood to yellow area of strip.',
     'false', null,
     'https://res.cloudinary.com/tjgtsydz/image/upload/v1790413006/medicart/medicines/dJRdaEcY6VEyfGXcFBuLjZQEzmqZpL8eUxMc8rDh_ios49s.webp',
     'Diabetic Care', 'Glucose Test Strips', 'Strip / Test Kit'),

    -- SubCat: Insulin Cartridge (2)
    ('NovoRapid Penfill',
     'Insulin Aspart 100IU/ml',
     'Novo Nordisk',
     '720.00', '1 cartridge of 3ml',
     'Fast-acting insulin analog for mealtime blood sugar control in diabetes.',
     'Inject subcutaneously 5-10 min before meals as prescribed. Keep refrigerated.',
     'true', null,
     'https://res.cloudinary.com/tjgtsydz/image/upload/v1790413037/medicart/medicines/novorapid_flexpen_pgasoj.jpg',
     'Diabetic Care', 'Insulin Cartridge', 'Injection / Vial'),

    ('NovoMix 30 Penfill',
     'Biphasic Insulin Aspart 30% + 70% 100IU/ml',
     'Novo Nordisk',
     '880.00', '1 cartridge of 3ml',
     'Biphasic insulin for twice-daily dosing — covers both mealtime and basal needs.',
     'Inject subcutaneously before breakfast and dinner as prescribed. Shake gently.',
     'true', null,
     'https://res.cloudinary.com/tjgtsydz/image/upload/v1790413083/medicart/medicines/novomix_pk6snb.jpg',
     'Diabetic Care', 'Insulin Cartridge', 'Injection / Vial'),

    -- SubCat: Insulin Pen (Onetime) (2)
    ('Tresiba FlexTouch',
     'Insulin Degludec 100IU/ml',
     'Novo Nordisk',
     '2750.00', '1 prefilled pen of 3ml',
     'Ultra-long-acting basal insulin pen — once daily at any time of day.',
     'Once daily subcutaneous injection at same time each day as prescribed.',
     'true', null,
     'https://res.cloudinary.com/tjgtsydz/image/upload/v1790413108/medicart/medicines/Tresiba-2490_sbmelo.jpg',
     'Diabetic Care', 'Insulin Pen (Onetime)', 'Pen / Device'),

    ('Trulicity 0.75mg',
     'Dulaglutide 0.75mg',
     'Eli Lilly',
     '4000.00', '1 prefilled pen',
     'GLP-1 receptor agonist pen for Type 2 diabetes — once weekly injection.',
     'Once weekly subcutaneous injection in abdomen, thigh, or upper arm as prescribed.',
     'true', null,
     'https://res.cloudinary.com/tjgtsydz/image/upload/v1790413131/medicart/medicines/T37kRbiWZx1p2TdeXotC9uZIZA2A1OulMTreTMMp_o8grhl.png',
     'Diabetic Care', 'Insulin Pen (Onetime)', 'Pen / Device'),

    -- SubCat: Lancets (2)
    ('Accu-Chek Softclix Lancets 25pcs',
     'Sterile Lancets 28G',
     'Roche Diabetes Care',
     '250.00', '1 pack of 25 lancets',
     'Ultra-thin 28G lancets for virtually painless fingertip blood sampling.',
     'Load into lancing device. Set depth. Press against fingertip and release.',
     'false', null,
     'https://res.cloudinary.com/tjgtsydz/image/upload/v1790413236/medicart/medicines/images_wzaiyf.jpg',
     'Diabetic Care', 'Lancets', 'Strip / Test Kit'),

    ('BD Ultra-Fine Lancets 100pcs',
     'Sterile Lancets 31G',
     'Becton Dickinson',
     '400.00', '1 box of 100 lancets',
     'Extra-fine 31G lancets compatible with most lancing devices.',
     'Load into compatible lancing device. Single use only.',
     'false', null,
     'https://res.cloudinary.com/tjgtsydz/image/upload/v1790413305/medicart/medicines/images_dtdzux.jpg',
     'Diabetic Care', 'Lancets', 'Strip / Test Kit'),

    -- SubCat: Diabetes Medicines (2) — one expensive brand
    ('Empa 10mg',
     'Empagliflozin 10mg',
     'Boehringer Ingelheim',
     '375.00', '1 strip of 15 tablets',
     'SGLT-2 inhibitor for Type 2 diabetes — also reduces cardiovascular risk.',
     'Adults: 10mg once daily in the morning with or without food.',
     'true', null,
     'https://res.cloudinary.com/tjgtsydz/image/upload/v1790413337/medicart/medicines/2QvQcPX6GKJHJjz0AXLKv5IwEknkCyR8ZcGcmzBC_vsiohz.jpg',
     'Diabetic Care', 'Diabetes Medicines', 'Tablet / Caplet'),

    ('Jardiance 10mg',
     'Empagliflozin 10mg',
     'Eli Lilly & Boehringer Ingelheim',
     '1050.00', '1 strip of 10 tablets',
     'Brand-name SGLT-2 inhibitor for blood glucose control and heart protection.',
     'Adults: 10mg once daily. May be increased to 25mg as prescribed.',
     'true', null,
     'https://res.cloudinary.com/tjgtsydz/image/upload/v1790413366/medicart/medicines/yqmyIg86vr0e63O07FY5gvVslp14RtgUZu3OwBCq_lctmbx.png',
     'Diabetic Care', 'Diabetes Medicines', 'Tablet / Caplet'),

    -- ==============================================================
    -- CATEGORY: Women's Care
    -- ==============================================================

    -- SubCat: Sanitary Pad (2)
    ('Freedom Heavy Flow Wings 16pads',
     'Sanitary Napkin — Heavy Flow',
     'Freedom (ACI Consumer Brands)',
     '200.00', '1 pack of 16 pads',
     'Heavy flow sanitary napkin with wings for maximum protection.',
     'Change every 4-6 hours or as needed. Dispose hygienically.',
     'false', null,
     'https://res.cloudinary.com/tjgtsydz/image/upload/v1788420004/medicart/medicines/augment_msiopf.jpg',
     'Women''s Care', 'Sanitary Pad', 'Sanitary Pad'),

    ('Senora Regular Flow 10pads',
     'Sanitary Napkin — Regular Flow',
     'Senora (Bashundhara Group)',
     '100.00', '1 pack of 10 pads',
     'Regular flow sanitary napkin with belt system for everyday comfort.',
     'Change every 4-6 hours. Dispose in dustbin — do not flush.',
     'false', null,
     'https://res.cloudinary.com/tjgtsydz/image/upload/v1788420004/medicart/medicines/augment_msiopf.jpg',
     'Women''s Care', 'Sanitary Pad', 'Sanitary Pad'),

    -- SubCat: Birth Control Pill (2)
    ('Femicon',
     'Ethinylestradiol 30mcg + Levonorgestrel 150mcg',
     'ACI Limited',
     '45.50', '1 box of 21 tablets',
     'Combined oral contraceptive pill for family planning.',
     'Take 1 tablet daily for 21 days starting day 1 of cycle, then 7-day break.',
     'true', null,
     'https://res.cloudinary.com/tjgtsydz/image/upload/v1788420004/medicart/medicines/augment_msiopf.jpg',
     'Women''s Care', 'Birth Control Pill', 'Tablet / Caplet'),

    ('Marvelon',
     'Ethinylestradiol 30mcg + Desogestrel 150mcg',
     'Organon Bangladesh',
     '105.00', '1 box of 21 tablets',
     'Low-dose combined oral contraceptive with good cycle control.',
     'Take 1 tablet daily at the same time for 21 days, then 7-day break.',
     'true', null,
     'https://res.cloudinary.com/tjgtsydz/image/upload/v1788420004/medicart/medicines/augment_msiopf.jpg',
     'Women''s Care', 'Birth Control Pill', 'Tablet / Caplet'),

    -- SubCat: Pregnancy Test (2)
    ('Pregna News Cassette',
     'HCG Urine Pregnancy Test',
     'Guangzhou Wondfo Biotech',
     '65.00', '1 cassette test',
     'Sensitive HCG-based pregnancy test cassette — results in 3 minutes.',
     'Collect urine in clean cup. Add 3 drops to cassette well. Read at 3 min.',
     'false', null,
     'https://res.cloudinary.com/tjgtsydz/image/upload/v1788420004/medicart/medicines/augment_msiopf.jpg',
     'Women''s Care', 'Pregnancy Test', 'Strip / Test Kit'),

    ('Get Sure HCG Test',
     'HCG Urine Pregnancy Test Cassette',
     'Get Sure Diagnostics',
     '80.00', '1 cassette test',
     'Fast and reliable home pregnancy test cassette with high sensitivity.',
     'Collect morning urine. Dip cassette or add drops. Read result at 5 minutes.',
     'false', null,
     'https://res.cloudinary.com/tjgtsydz/image/upload/v1788420004/medicart/medicines/augment_msiopf.jpg',
     'Women''s Care', 'Pregnancy Test', 'Strip / Test Kit'),

    -- SubCat: Beauty Care (2)
    ('Bobcare Ointment',
     'Herbal Skin Care Ointment',
     'ACI Consumer Brands',
     '300.00', '1 tube of 30g',
     'Herbal ointment for dry, cracked, or irritated skin — soothing and moisturising.',
     'Apply thin layer to affected area 2-3 times daily or as needed.',
     'false', null,
     'https://res.cloudinary.com/tjgtsydz/image/upload/v1788420004/medicart/medicines/augment_msiopf.jpg',
     'Women''s Care', 'Beauty Care', 'Cream / Ointment'),

    ('Wonica Hair Removal Cream',
     'Thioglycolate-based Hair Removal Cream',
     'Wonica Cosmetics',
     '1500.00', '1 tube of 30g',
     'Painless facial and body hair removal cream. Results in 5-8 minutes.',
     'Apply thick layer to dry skin. Leave 5-8 min. Wipe off with damp cloth.',
     'false', null,
     'https://res.cloudinary.com/tjgtsydz/image/upload/v1788420004/medicart/medicines/augment_msiopf.jpg',
     'Women''s Care', 'Beauty Care', 'Cream / Ointment')

) AS m(
    "Name", "GenericName", "Manufacturer",
    "Price", "Unit", "Description", "Dosage",
    "RequiresPrescription", "SensitivityLevel", "ImageUrl",
    "CategoryName", "SubCategoryName", "ProductTypeName"
)
JOIN "Categories" cat ON cat."Name" = m."CategoryName"
JOIN "SubCategories" sub
    ON sub."Name" = m."SubCategoryName"
    AND sub."CategoryId" = cat."Id"
JOIN "ProductTypes" pt ON pt."Name" = m."ProductTypeName"
WHERE NOT EXISTS (
    SELECT 1 FROM "Medicines" existing
    WHERE existing."Name" = m."Name"
);


-- ============================================================
-- STOCKS
-- Expiry dates are relative to today (2026-09-26):
--   Normal           : 2027-06-30 onwards
--   Warning (8-30d)  : 2026-10-10 (Napa Extra), 2026-10-20 (Metform 500mg)
--   Critical (0-7d)  : 2026-09-30 (Augment 500), 2026-10-02 (Freedom Heavy)
--   Expired          : 2026-09-01 (Cloma 2)
--
-- Qty:
--   Normal    : 30-120
--   Low stock : 4-8
--   Out stock : 0
-- ============================================================
 
INSERT INTO "Stocks" ("MedicineId", "Quantity", "ExpiryDate", "UpdatedAt")
SELECT med."Id", s."Quantity"::int, s."ExpiryDate"::date, NOW()
FROM (VALUES
    -- Allergies & Asthma
    ('Fexo 120',                    8,   '2027-12-31'),  -- LOW STOCK
    ('Montela 10mg',               60,   '2027-11-30'),  -- Normal
 
    -- Epilepsy & Neurological
    ('Gaba 300mg',                  0,   '2027-08-31'),  -- OUT OF STOCK + high sensitivity
    ('Cazep 200mg',                45,   '2027-09-30'),  -- Normal + mid sensitivity
 
    -- Pain Relief
    ('Napa Extra',                 80,   '2026-10-10'),  -- WARNING EXPIRY (14 days)
    ('Arlin 600',                  50,   '2027-06-30'),  -- Normal
 
    -- Gastrointestinal
    ('Nexum 40',                   35,   '2027-05-31'),  -- Normal
    ('Lanso D',                    30,   '2027-04-30'),  -- Normal
 
    -- Antibiotics
    ('Augment 500',                25,   '2026-09-30'),  -- CRITICAL EXPIRY (4 days) BLOCKED
    ('Zimax 500',                  40,   '2027-07-31'),  -- Normal
 
    -- Cardiac & Blood Pressure
    ('Bislol 5mg',                 55,   '2027-10-31'),  -- Normal
    ('Amlor 5mg',                  70,   '2027-12-31'),  -- Normal
 
    -- Diabetes (Medicine cat)
    ('Metform 500mg',             120,   '2026-10-20'),  -- WARNING EXPIRY (24 days) + low sens
    ('Glucophage 500mg',           90,   '2027-08-31'),  -- Normal + low sensitivity
 
    -- Hormonal & Endocrine
    ('Thyronorm 50mcg',            50,   '2027-11-30'),  -- Normal
    ('Cloma 2',                     0,   '2026-09-01'),  -- OUT OF STOCK + EXPIRED + high sens
 
    -- Mental Health
    ('Serenace 5mg',               30,   '2027-06-30'),  -- Normal
    ('Flunil 20mg',                40,   '2027-09-30'),  -- Normal
 
    -- Vitamins & Supplements
    ('Supravit-S',                 80,   '2028-01-31'),  -- Normal
    ('Revital Woman',              20,   '2027-12-31'),  -- Normal
    ('Caltrate 600+D',             15,   '2027-10-31'),  -- Normal
    ('Biovit B Complex',          100,   '2028-03-31'),  -- Normal
    ('NOW Omega-3 1000mg',         25,   '2027-08-31'),  -- Normal
    ('Truemed Turmeric',           18,   '2027-11-30'),  -- Normal
    ('Basak Syrup',                 5,   '2027-06-30'),  -- LOW STOCK
    ('Gintex 500mg',               45,   '2027-09-30'),  -- Normal
    ('Protinex Chocolate',         12,   '2027-07-31'),  -- Normal
    ('Powerlift Weight Gainer',     7,   '2027-05-31'),  -- LOW STOCK
 
    -- Diabetic Care
    ('Accu-Chek Active',           15,   '2028-04-30'),  -- Normal (device)
    ('GlucoSure Star',             10,   '2028-06-30'),  -- Normal
    ('Accu-Chek Strips 50pcs',     30,   '2027-08-31'),  -- Normal
    ('OneTouch Verio Strips 50pcs', 20,  '2027-10-31'),  -- Normal
    ('NovoRapid Penfill',          18,   '2027-04-30'),  -- Normal
    ('NovoMix 30 Penfill',          4,   '2027-03-31'),  -- LOW STOCK
    ('Tresiba FlexTouch',          12,   '2027-06-30'),  -- Normal
    ('Trulicity 0.75mg',            8,   '2027-05-31'),  -- LOW STOCK
    ('Accu-Chek Softclix Lancets 25pcs', 50, '2028-01-31'), -- Normal
    ('BD Ultra-Fine Lancets 100pcs',     35, '2028-03-31'), -- Normal
    ('Empa 10mg',                  45,   '2027-11-30'),  -- Normal
    ('Jardiance 10mg',             20,   '2027-09-30'),  -- Normal
 
    -- Women's Care
    ('Freedom Heavy Flow Wings 16pads',  0, '2026-10-02'),  -- OUT OF STOCK + CRITICAL EXPIRY
    ('Senora Regular Flow 10pads',      60, '2027-12-31'),  -- Normal
    ('Femicon',                    6,   '2027-08-31'),  -- LOW STOCK
    ('Marvelon',                   4,   '2027-07-31'),  -- LOW STOCK
    ('Pregna News Cassette',       50,   '2027-06-30'),  -- Normal
    ('Get Sure HCG Test',          45,   '2027-05-31'),  -- Normal
    ('Bobcare Ointment',           25,   '2027-10-31'),  -- Normal
    ('Wonica Hair Removal Cream',  15,   '2027-08-31')   -- Normal
 
) AS s("MedicineName", "Quantity", "ExpiryDate")
JOIN "Medicines" med ON med."Name" = s."MedicineName"
ON CONFLICT ("MedicineId") DO NOTHING;
 

-- ============================================================
-- SIDE EFFECTS
-- mild   : common, non-serious
-- moderate : needs monitoring
-- severe : rare but serious — requires Rx justification
-- No side effects: devices, test kits, pads, hair cream
-- ============================================================
 
INSERT INTO "SideEffects" ("MedicineId", "Effect", "Severity")
SELECT med."Id", se."Effect", se."Severity"
FROM (VALUES
 
    -- Fexo 120 (mild)
    ('Fexo 120',        'Headache',                       'mild'),
    ('Fexo 120',        'Dry mouth',                      'mild'),
 
    -- Montela 10mg (mild + moderate)
    ('Montela 10mg',    'Headache',                       'mild'),
    ('Montela 10mg',    'Abdominal pain',                 'moderate'),
 
    -- Gaba 300mg (mild + moderate + severe)
    ('Gaba 300mg',      'Dizziness and drowsiness',       'mild'),
    ('Gaba 300mg',      'Difficulty with coordination',   'moderate'),
    ('Gaba 300mg',      'Mood changes or suicidal thoughts', 'severe'),
 
    -- Cazep 200mg (mild + severe)
    ('Cazep 200mg',     'Dizziness',                      'mild'),
    ('Cazep 200mg',     'Nausea',                         'mild'),
    ('Cazep 200mg',     'Severe skin rash (Stevens-Johnson Syndrome)', 'severe'),
 
    -- Napa Extra (mild)
    ('Napa Extra',      'Nausea',                         'mild'),
    ('Napa Extra',      'Insomnia due to caffeine',       'mild'),
 
    -- Arlin 600 (mild + moderate)
    ('Arlin 600',       'Stomach upset',                  'mild'),
    ('Arlin 600',       'Heartburn',                      'mild'),
    ('Arlin 600',       'Gastrointestinal bleeding',      'moderate'),
 
    -- Nexum 40 (mild + moderate)
    ('Nexum 40',        'Headache',                       'mild'),
    ('Nexum 40',        'Abdominal pain',                 'moderate'),
 
    -- Lanso D (mild)
    ('Lanso D',         'Dry mouth',                      'mild'),
    ('Lanso D',         'Dizziness',                      'mild'),
 
    -- Augment 500 (mild + moderate)
    ('Augment 500',     'Diarrhea',                       'moderate'),
    ('Augment 500',     'Nausea',                         'mild'),
    ('Augment 500',     'Skin rash',                      'moderate'),
 
    -- Zimax 500 (mild + moderate)
    ('Zimax 500',       'Nausea',                         'mild'),
    ('Zimax 500',       'Diarrhea',                       'moderate'),
 
    -- Bislol 5mg (mild + moderate)
    ('Bislol 5mg',      'Fatigue',                        'mild'),
    ('Bislol 5mg',      'Cold hands and feet',            'mild'),
    ('Bislol 5mg',      'Bradycardia (slow heart rate)',  'moderate'),
 
    -- Amlor 5mg (mild)
    ('Amlor 5mg',       'Ankle swelling',                 'mild'),
    ('Amlor 5mg',       'Flushing',                       'mild'),
 
    -- Metform 500mg (mild)
    ('Metform 500mg',   'Nausea and vomiting',            'mild'),
    ('Metform 500mg',   'Diarrhea',                       'mild'),
 
    -- Glucophage 500mg (mild)
    ('Glucophage 500mg','Nausea',                         'mild'),
    ('Glucophage 500mg','Metallic taste',                 'mild'),
 
    -- Thyronorm 50mcg (mild + moderate)
    ('Thyronorm 50mcg', 'Heart palpitations if overdosed','moderate'),
    ('Thyronorm 50mcg', 'Insomnia',                       'mild'),
 
    -- Cloma 2 (mild + moderate + severe)
    ('Cloma 2',         'Hot flashes',                    'mild'),
    ('Cloma 2',         'Abdominal bloating',             'moderate'),
    ('Cloma 2',         'Ovarian hyperstimulation syndrome', 'severe'),
 
    -- Serenace 5mg (moderate + severe)
    ('Serenace 5mg',    'Drowsiness',                     'mild'),
    ('Serenace 5mg',    'Muscle stiffness (extrapyramidal effects)', 'moderate'),
    ('Serenace 5mg',    'Tardive dyskinesia with long-term use', 'severe'),
 
    -- Flunil 20mg (mild + moderate + severe)
    ('Flunil 20mg',     'Nausea',                         'mild'),
    ('Flunil 20mg',     'Insomnia',                       'mild'),
    ('Flunil 20mg',     'Increased suicidal ideation in first weeks', 'severe'),
 
    -- Supravit-S (mild)
    ('Supravit-S',      'Mild nausea on empty stomach',   'mild'),
 
    -- Revital Woman (mild)
    ('Revital Woman',   'Mild stomach upset',             'mild'),
 
    -- Caltrate 600+D (mild)
    ('Caltrate 600+D',  'Constipation',                   'mild'),
    ('Caltrate 600+D',  'Bloating',                       'mild'),
 
    -- Biovit B Complex (mild)
    ('Biovit B Complex','Urine discoloration (harmless)', 'mild'),
 
    -- NOW Omega-3 (mild)
    ('NOW Omega-3 1000mg', 'Fishy aftertaste',            'mild'),
    ('NOW Omega-3 1000mg', 'Mild nausea',                 'mild'),
 
    -- Truemed Turmeric (mild)
    ('Truemed Turmeric','Stomach upset in high doses',    'mild'),
 
    -- Basak Syrup (mild)
    ('Basak Syrup',     'Mild drowsiness',                'mild'),
 
    -- Gintex (mild)
    ('Gintex 500mg',    'Headache',                       'mild'),
    ('Gintex 500mg',    'Digestive upset',                'mild'),
 
    -- Protinex (mild)
    ('Protinex Chocolate', 'Bloating if lactose sensitive', 'mild'),
 
    -- NovoRapid (moderate + severe)
    ('NovoRapid Penfill','Hypoglycemia (low blood sugar)', 'moderate'),
    ('NovoRapid Penfill','Severe hypoglycemia with overdose', 'severe'),
 
    -- NovoMix (moderate + severe)
    ('NovoMix 30 Penfill','Hypoglycemia',                 'moderate'),
    ('NovoMix 30 Penfill','Injection site reaction',      'mild'),
 
    -- Tresiba FlexTouch (moderate)
    ('Tresiba FlexTouch','Hypoglycemia',                  'moderate'),
    ('Tresiba FlexTouch','Injection site lipodystrophy',  'mild'),
 
    -- Trulicity (mild + moderate)
    ('Trulicity 0.75mg','Nausea',                         'mild'),
    ('Trulicity 0.75mg','Vomiting',                       'moderate'),
 
    -- Empa 10mg (mild + moderate)
    ('Empa 10mg',       'Urinary tract infection',        'mild'),
    ('Empa 10mg',       'Genital yeast infection',        'moderate'),
 
    -- Jardiance 10mg (mild + moderate)
    ('Jardiance 10mg',  'Increased urination',            'mild'),
    ('Jardiance 10mg',  'Urinary tract infection',        'moderate'),
 
    -- Femicon (mild + moderate)
    ('Femicon',         'Nausea',                         'mild'),
    ('Femicon',         'Mood changes',                   'mild'),
    ('Femicon',         'Increased blood pressure risk',  'moderate'),
 
    -- Marvelon (mild + moderate)
    ('Marvelon',        'Headache',                       'mild'),
    ('Marvelon',        'Breast tenderness',              'mild'),
    ('Marvelon',        'Thromboembolism risk',           'moderate'),
 
    -- Cloma 2 already done above
 
    -- Bobcare Ointment (mild)
    ('Bobcare Ointment','Mild skin irritation',           'mild')
 
    -- No side effects for:
    -- Accu-Chek Active, GlucoSure Star, Accu-Chek Strips, OneTouch Strips,
    -- Accu-Chek Lancets, BD Lancets, Freedom Pads, Senora Pads,
    -- Pregna News, Get Sure HCG, Wonica Cream, Powerlift, Thyronorm already done
 
) AS se("MedicineName", "Effect", "Severity")
JOIN "Medicines" med ON med."Name" = se."MedicineName"
WHERE NOT EXISTS (
    SELECT 1 FROM "SideEffects" existing
    WHERE existing."MedicineId" = med."Id"
      AND existing."Effect" = se."Effect"
);
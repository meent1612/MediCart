-- ============================================================
-- MediCart seed data
-- Run this in Neon SQL Editor after dotnet ef database update
-- ============================================================

-- ============================================================
-- 1. Divisions (8 official divisions of Bangladesh)
-- ============================================================

INSERT INTO "Divisions" ("Name") VALUES
    ('Dhaka'),
    ('Chattogram'),
    ('Khulna'),
    ('Rajshahi'),
    ('Barishal'),
    ('Sylhet'),
    ('Rangpur'),
    ('Mymensingh')
ON CONFLICT DO NOTHING;

-- ============================================================
-- 2. Cities (mapped to DivisionId by subquery)
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
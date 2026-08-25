# MediCart

**An Online Pharmacy Platform with Admin-Supervised Ordering**

CSE 3200 – Software Development V | Lab Section A2 | Group A203
Department of Computer Science and Engineering, AUST

## Team

| Student ID | Name |
|---|---|
| 20230104028 | Rahnuma Azra Mahjabin |
| 20230104032 | Farzana Mim |
| 20230104043 | Shayma Sharmeen |
| 20220204020 | Zumaina Tahsin |

**Course Instructors:** Md. Hasan Al Kayem, Tanjila Broti (Lecturers, Department of CSE)

---

## About The Project

Existing online pharmacy platforms in Bangladesh handle catalogue browsing, search, and delivery well, but order review is usually narrow — triggered only when a specific medicine is tagged as prescription-required. This means most orders ship without any human check, and there is no built-in way to notice unusual order patterns (e.g. a large or repeated purchase of a sensitive medicine).

**MediCart** closes this gap by making admin review the default for **every** order, not just prescription orders. Every order is queued for admin approval, and the system automatically flags unusually large orders of sensitive medicines based on a configurable sensitivity tier. This is paired with two-tier expiry alerts, low-stock alerts, and a per-admin audit log, turning the admin dashboard into a genuine oversight tool rather than a simple order log.

### Aligned Sustainable Development Goal

- **Primary:** SDG 3 – Good Health and Well-being (consistent human safety check on every order, side-effect visibility, automatic flagging of unusually large sensitive-medicine orders)
- **Secondary:** SDG 10 – Reduced Inequalities, SDG 12 – Responsible Consumption and Production, SDG 9 – Industry, Innovation and Infrastructure

### Objectives

- Make admin review the default for every order, not only those tagged as requiring a prescription.
- Automatically flag unusually large orders of sensitive medicines by a configurable sensitivity tier.
- Give the admin dashboard two-tier expiry alerts and low-stock alerts before problems reach customers.
- Give every admin action a traceable audit trail across the shared admin dashboard.

---

## Key Features

### Guest (No Login Required)
- View the **About Us** page
- View the **FAQ** page (e.g. when a prescription is required)
- Submit a message through the **Contact Us** form without creating an account
- Register for a new account

### Customer / User
- Register and log in
- Browse medicines by type (Tablet, Syrup, Injection, Ointment, Drops)
- Browse and filter medicines by medical category and condition-based use tag (e.g. fever, gastric)
- Search medicines by name and filter by price range
- View a medicine's detail page: price, stock status, and side effects with severity
- Add to cart, adjust quantities, remove items — cart persists across sessions
- Checkout: delivery address, division/city (with automatic delivery charge), payment method
- Upload a prescription image when the cart requires one
- Track order status: Pending, Processing, Shipped, Delivered, or Rejected (with reason)
- View/edit profile and full order history

### Admin (Shared Dashboard, Multiple Admin Accounts)
- Log in to a shared admin dashboard
- Add, edit, and delete medicines (type, category, manufacturer, price, stock, expiry, side effects, sensitivity tier)
- Maintain global Product Types and Categories reference lists
- Review every incoming order and approve, reject (with reason), mark Shipped, or mark Delivered
- View prescription images attached to orders before approving/rejecting
- View a **flagged-orders** list — orders crossing a sensitivity-tier quantity threshold are auto-highlighted:
  - High sensitivity ≥ 5 units
  - Mid sensitivity ≥ 15 units
  - Low sensitivity ≥ 30 units
- View a **two-tier expiry alert** list (30-day warning, 7-day critical) and update stock directly
- View a **low-stock alert** list against a configurable threshold
- View and mark incoming contact messages as read
- View a searchable **audit log** of admin actions (who, what, on which record, when)

---

## Tech Stack

| Layer | Technology |
|---|---|
| Presentation | Razor Views, Bootstrap 5 |
| Application | ASP.NET Core MVC (C#) — role-based authentication & authorization via ASP.NET Core Identity |
| Data / Database | Entity Framework Core (Code-First) → PostgreSQL |
| Database Hosting | Supabase (managed PostgreSQL) |
| Web Hosting | Azure App Service |
| IDE | Visual Studio Code |

## System Architecture

```
Presentation Layer   → Guest / Customer / Admin views (Razor + Bootstrap 5)
        ↓
Application Layer    → ASP.NET Core MVC controllers, business logic,
                        ASP.NET Core Identity, role-based authorization
        ↓
Core Modules          → Authentication & Profile
                        Catalogue & Search
                        Cart & Checkout
                        Order & Prescription
                        Stock & Expiry Alerts
                        Flagged Orders & Audit Log
        ↓
Data Access Layer     → Entity Framework Core (Code-First, Npgsql provider)
        ↓
Database              → PostgreSQL (Supabase, hosted & managed)
        ↓
Deployment            → Azure App Service (web) + Supabase (database)
```

The domain is highly relational — customers link to orders, orders link to medicines and prescriptions, medicines link to categories, product types, and side effects, and every admin action links back to a specific admin account via the audit log. A normalised PostgreSQL schema with foreign keys, transactions, and constraints (e.g. stock can never go negative) keeps this data consistent.

## Entity Relationship Overview

The schema is organised around a few clusters of entities:

- **Identity & access:** `Users`, `Admins`, `AuditLog`
- **Catalogue:** `Medicines`, `Categories` (self-referencing, for parent/sub-category), `ProductTypes`, `SideEffects` (many-to-many with `Medicines`)
- **Ordering:** `Cart`, `Orders`, `OrderItems`, `Prescriptions` (linked to orders that require one)
- **Delivery:** `Divisions`, `Cities` (city belongs to a division, sets the delivery charge)
- **Stock & alerts:** `Stock`, `ExpiryAlerts` (triggered off medicine stock/expiry data)
- **Support:** `ContactMessages`

Relationships worth calling out:
- A `User` places many `Orders`; each `Order` is delivered to one `City`, which belongs to one `Division`.
- An `Order` has many `OrderItems`, each pointing to one `Medicine`, and may have one `Prescription`.
- A `Medicine` belongs to one `Category` and one `ProductType`, and has many `SideEffects` (many-to-many) and a `Stock` record.
- An `Admin` performs many `AuditLog` entries; each entry records the action, the affected record, and the timestamp.
---

## License

This project is developed for academic purposes as part of the CSE 3200 course at the Ahsanullah University of Science and Technology (AUST).

# MediCart — PROJECT STATE (Read-Only Inventory)

> **Generated**: 2026-09-15  
> **Framework**: ASP.NET Core 10.0, Razor Views, EF Core Code-First, PostgreSQL/Neon, Identity, deployed on Azure  
> **Purpose**: Factual snapshot of what is actually implemented in code right now.

---

## 1. Roles & Auth

### 1.1 Roles Defined
Seeded in `Program.cs` lines 38-44:
- **Admin**
- **Customer**

### 1.2 Seed Accounts
- **4 Admin accounts** seeded (lines 52-79) with password `Admin@1234`, EmailConfirmed = true
- **11 Customer accounts** seeded (lines 87-126) with password `User@1234`, EmailConfirmed = true

### 1.3 Authorize Attributes per Controller

| Controller | `[Authorize]` Attribute | Notes |
|---|---|---|
| `AdminController` | `[Authorize(Roles = "Admin")]` (line 11) | Active — Admin only |
| `AdminOrdersController` | `[Authorize(Roles = "Admin")]` (line 11) | Active — Admin only |
| `CartController` | `[Authorize(Roles = "Customer")]` (line 9) | Active — Customer only |
| `CheckoutController` | `[Authorize(Roles = "Customer")]` (line 12) | Active — Customer only |
| `ConfirmationController` | `[Authorize(Roles = "Customer")]` (line 10) | Active — Customer only |
| `HomeController` | **None** | Public — guests, customers, and admins |
| `MedicinesController` | **None** | Public — anyone can browse |
| `OrdersController` | `[Authorize(Roles = "Customer")]` (line 9) | Active — Customer only |
| `UserProfileController` | `[Authorize]` (line 10) | Active — any authenticated user |

### 1.4 Registration Flow (Identity Razor Page `Register_cshtml.cs`)
- **Fields**: FullName, Phone, Email, Password, ConfirmPassword, AgreeToTerms
- **Validation**: Required on all fields; FullName 2-100 chars; Password min 8 chars; Email must be valid; AgreeToTerms must be true
- **Post-registration**: User auto-assigned "Customer" role, auto-signed-in, redirected to home
- **Email confirmation**: **Not required** (`options.SignIn.RequireConfirmedAccount = false` — Program.cs line 19)
- **Lockout**: `lockoutOnFailure: false` in Login (Login_cshtml.cs line 86) — **lockout is disabled**

### 1.5 Login Flow (Identity Razor Page `Login_cshtml.cs`)
- **Fields**: Email, Password, RememberMe
- **Post-login redirect**: Admin users → `/Admin/Dashboard`; Customer users → returnUrl or `/`
- **Error message**: "Invalid email or password." on failure

---

## 2. Controllers & Actions Inventory

### 2.1 HomeController (public)

| Action | Verb | Route | Parameters | Return | Mutates? |
|---|---|---|---|---|---|
| `Index` | GET | `/Home/Index` or `/` | — | View (with ViewBag: Categories, ProductTypes) | No |
| `Privacy` | GET | `/Home/Privacy` | — | View | No |
| `Error` | GET | `/Home/Error` | — | View(ErrorViewModel) | No |
| `About` | GET | `/Home/About` | — | View | No |
| `Contact` | GET | `/Home/Contact` | — | View | No |
| `Contact` | POST | `/Home/Contact` | ContactViewModel (form) | Redirect/View | **Yes** — saves ContactMessage |
| `Terms` | GET | `/Home/Terms` | — | View | No |
| `Loading` | GET | `/Home/Loading` | — | View | No |

### 2.2 MedicinesController (public)

| Action | Verb | Route | Parameters | Return | Mutates? |
|---|---|---|---|---|---|
| `Index` | GET | `/Medicines` | — | View(List\<MedicineViewModel\>) | No |

### 2.3 CartController (`[Authorize(Roles = "Customer")]`)

| Action | Verb | Route | Parameters | Return | Mutates? |
|---|---|---|---|---|---|
| `Index` | GET | `/Cart` | — | View(List\<CartItemViewModel\>) | No |
| `Add` | POST | `/Cart/Add` | medicineId (int), quantity (int, default 1) | JSON (Ok/BadRequest) | **Yes** — modifies CartItems + Stock |
| `UpdateQuantity` | POST | `/Cart/UpdateQuantity` | cartItemId (int), newQuantity (int) | JSON (Ok/BadRequest) | **Yes** — modifies CartItems + Stock |
| `Remove` | POST | `/Cart/Remove` | cartItemId (int) | JSON (Ok/BadRequest) | **Yes** — removes CartItem, restores Stock |
| `Count` | GET | `/Cart/Count` | — | JSON { cartItemCount } | No |

### 2.4 CheckoutController (`[Authorize(Roles = "Customer")]`)

| Action | Verb | Route | Parameters | Return | Mutates? |
|---|---|---|---|---|---|
| `Index` | GET | `/Checkout` | — | View(CheckoutViewModel) | No |
| `SendOtp` | POST | `/Checkout/SendOtp` | — | JSON (Ok/StatusCode 500) | **Yes** — invalidates old OTPs, creates new OtpCode, sends email |
| `VerifyOtp` | POST | `/Checkout/VerifyOtp` | VerifyOtpRequest (JSON body: Code) | JSON (Ok/BadRequest) | **Yes** — marks OTP as used |
| `PlaceOrder` | POST | `/Checkout/PlaceOrder` | divisionId, cityId, addressLine, phone, paymentMethod, prescriptionFile (IFormFile?) | JSON (Ok/BadRequest/StatusCode 500) | **Yes** — creates Order, OrderItems, Prescription, Payment, clears cart |

### 2.5 ConfirmationController (`[Authorize(Roles = "Customer")]`)

| Action | Verb | Route | Parameters | Return | Mutates? |
|---|---|---|---|---|---|
| `Index` | GET | `/Confirmation/{id:int}` | id (int) | View(OrderConfirmationViewModel) | No |

### 2.6 OrdersController (`[Authorize(Roles = "Customer")]`)

| Action | Verb | Route | Parameters | Return | Mutates? |
|---|---|---|---|---|---|
| `Detail` | GET | `/Orders/Detail/{id}` | id (int) | View(OrderConfirmationViewModel) | No |

### 2.7 UserProfileController (`[Authorize]`)

| Action | Verb | Route | Parameters | Return | Mutates? |
|---|---|---|---|---|---|
| `Index` | GET | `/UserProfile` | — | View(UserProfileViewModel) | No |
| `Index` | POST | `/UserProfile` | UserProfileViewModel (form) | View/Redirect | **Yes** — updates FullName, PhoneNumber |

### 2.8 AdminController (`[Authorize(Roles = "Admin")]`)

| Action | Verb | Route | Parameters | Return | Mutates? |
|---|---|---|---|---|---|
| `Profile` | GET | `/Admin/Profile` | — | View(AdminProfileViewModel) | No |
| `UpdateProfile` | POST | `/Admin/UpdateProfile` | AdminProfileFormViewModel | Redirect | **Yes** — updates admin user |
| `Medicines` | GET | `/Admin/Medicines` | search?, categoryId?, subCategoryId?, productTypeId? | View(AdminMedicinesPageViewModel) | No |
| `Dashboard` | GET | `/Admin/Dashboard` | — | View(AdminDashboardViewModel) | No |
| `StockExpiry` | GET | `/Admin/StockExpiry` | filter? | View(AdminStockExpiryViewModel) | No |
| `AuditLog` | GET | `/Admin/AuditLog` | search?, actionType?, fromDate?, toDate?, page? | View(AdminAuditLogViewModel) | No |
| `ContactMessages` | GET | `/Admin/ContactMessages` | status? | View(AdminContactMessageListViewModel) | No |
| `MarkMessageAsRead` | POST | `/Admin/MarkMessageAsRead` | id (int) | Redirect | **Yes** — marks message read, writes AuditLog |
| `Categories` | GET | `/Admin/Categories` | — | View(AdminCategoriesViewModel) | No |
| `GetSubCategories` | GET | `/Admin/GetSubCategories` | categoryId (int) | JSON (List\<SubCategoryOptionViewModel\>) | No |
| `CreateCategory` | POST | `/Admin/CreateCategory` | CategoryFormViewModel | Redirect | **Yes** — adds Category or SubCategory |
| `EditCategory` | POST | `/Admin/EditCategory` | CategoryFormViewModel | Redirect | **Yes** — edits Category or SubCategory |
| `DeleteCategory` | POST | `/Admin/DeleteCategory` | id (int) | Redirect | **Yes** — deletes Category |
| `DeleteSubCategory` | POST | `/Admin/DeleteSubCategory` | id (int) | Redirect | **Yes** — deletes SubCategory |
| `CreateProductType` | POST | `/Admin/CreateProductType` | ProductTypeFormViewModel | Redirect | **Yes** — adds ProductType |
| `EditProductType` | POST | `/Admin/EditProductType` | ProductTypeFormViewModel | Redirect | **Yes** — edits ProductType |
| `DeleteProductType` | POST | `/Admin/DeleteProductType` | id (int) | Redirect | **Yes** — deletes ProductType |
| `AddMedicine` | GET | `/Admin/AddMedicine` | — | View(AdminMedicineFormPageViewModel) | No |
| `AddMedicine` | POST | `/Admin/AddMedicine` | AdminMedicineFormPageViewModel | Redirect/View | **Yes** — adds Medicine + Stock + SideEffects, uploads image |
| `EditMedicine` | GET | `/Admin/EditMedicine/{id}` | id (int) | View("AddMedicine", model) | No |
| `EditMedicine` | POST | `/Admin/EditMedicine` | MedicineFormViewModel | Redirect/View | **Yes** — updates Medicine + Stock + SideEffects |
| `DeleteMedicine` | POST | `/Admin/DeleteMedicine` | id (int) | Redirect | **Yes** — deletes Medicine (blocked if order history exists) |

### 2.9 AdminOrdersController (`[Authorize(Roles = "Admin")]`)

| Action | Verb | Route | Parameters | Return | Mutates? |
|---|---|---|---|---|---|
| `IncomingOrders` | GET | `/AdminOrders/IncomingOrders` | status? | View(AdminOrderListViewModel) | No |
| `OrderDetail` | GET | `/AdminOrders/OrderDetail/{id}` | id (int) | View(AdminOrderDetailViewModel) | No |
| `Approve` | POST | `/AdminOrders/Approve` | id (int) | Redirect | **Yes** — sets Status to "Processing", writes AuditLog |
| `Reject` | POST | `/AdminOrders/Reject` | id (int), reason (string) | Redirect | **Yes** — sets Status to "Rejected", writes AuditLog |
| `MarkShipped` | POST | `/AdminOrders/MarkShipped` | id (int) | Redirect | **Yes** — sets Status to "Shipped", writes AuditLog |
| `MarkDelivered` | POST | `/AdminOrders/MarkDelivered` | id (int) | Redirect | **Yes** — sets Status to "Delivered", writes AuditLog |
| `FlaggedOrders` | GET | `/AdminOrders/FlaggedOrders` | tier? | View(AdminFlaggedOrdersListViewModel) | No |

---

## 3. Models & ViewModels

### 3.1 EF Core Entities (Data/)

| Entity | Properties | Validation Attributes |
|---|---|---|
| **ApplicationUser** (extends IdentityUser) | `FullName` (string) | ⚠️ None on entity |
| **Division** | `Id` (int), `Name` (string), `DeliveryCharge` (decimal, default 120.00m) | ⚠️ None |
| **City** | `Id` (int), `DivisionId` (int), `Name` (string) | ⚠️ None (uniqueness via Fluent API index) |
| **Category** | `Id` (int), `Name` (string), `Description` (string?), `CreatedAt` (DateTime) | ⚠️ None |
| **SubCategory** | `Id` (int), `CategoryId` (int), `Name` (string), `Description` (string?), `CreatedAt` (DateTime) | ⚠️ None |
| **ProductType** | `Id` (int), `Name` (string) | ⚠️ None |
| **Medicine** | `Id` (int), `CategoryId` (int), `SubCategoryId` (int?), `ProductTypeId` (int), `Name` (string), `GenericName` (string?), `Manufacturer` (string?), `Price` (decimal), `Unit` (string?), `Description` (string?), `Dosage` (string?), `RequiresPrescription` (bool), `SensitivityLevel` (string?), `ImageUrl` (string?), `CreatedAt` (DateTime) | ⚠️ None (CHECK constraint via Fluent API for SensitivityLevel) |
| **SideEffect** | `Id` (int), `MedicineId` (int), `Effect` (string), `Severity` (string) | ⚠️ None (CHECK constraint via Fluent API for Severity) |
| **Stock** | `Id` (int), `MedicineId` (int), `Quantity` (int, default 0), `ExpiryDate` (DateOnly), `UpdatedAt` (DateTime) | ⚠️ None |
| **CartItem** | `Id` (int), `UserId` (string), `MedicineId` (int), `Quantity` (int), `AddedAt` (DateTime), `UpdatedAt` (DateTime) | ⚠️ None |
| **Order** | `Id` (int), `UserId` (string), `TotalAmount` (decimal), `DivisionId` (int), `CityId` (int), `AddressLine` (string), `DeliveryCharge` (decimal), `Phone` (string), `Status` (string, default "Pending"), `IsFlagged` (bool), `RejectionReason` (string?), `PaymentMethod` (string?), `CreatedAt` (DateTime) | ⚠️ None (CHECK constraint via Fluent API for Status) |
| **OrderItem** | `Id` (int), `OrderId` (int), `MedicineId` (int), `Quantity` (int), `UnitPrice` (decimal) | ⚠️ None |
| **Prescription** | `Id` (int), `UserId` (string), `OrderId` (int), `ImageUrl` (string), `Status` (string, default "pending"), `UploadedAt` (DateTime) | ⚠️ None (CHECK constraint via Fluent API for Status) |
| **Payment** | `Id` (int), `OrderId` (int), `UserId` (string), `Amount` (decimal), `Method` (string), `Status` (string, default "pending"), `PaidAt` (DateTime?), `CreatedAt` (DateTime) | ⚠️ None (CHECK constraint via Fluent API for Status) |
| **Notification** | `Id` (int), `UserId` (string), `Message` (string), `IsRead` (bool), `CreatedAt` (DateTime) | ⚠️ None |
| **ContactMessage** | `Id` (int), `Name` (string), `Email` (string), `Message` (string), `IsRead` (bool), `CreatedAt` (DateTime), `UserId` (string?) | ⚠️ None |
| **AuditLog** | `Id` (int), `AdminId` (string), `Action` (string), `TableName` (string?), `RecordId` (int?), `CreatedAt` (DateTime) | ⚠️ None |
| **OtpCode** | `Id` (int), `Email` (string), `Code` (string), `ExpiresAt` (DateTime), `IsUsed` (bool), `CreatedAt` (DateTime) | ⚠️ None |

> **Note on Deleted Entities**: The `ExpiryAlert` entity does **not** exist and must not be referenced. `Data/ExpiryAlert.cs`, the `DbSet<ExpiryAlert>`, its check constraint in `OnModelCreating`, and the `ICollection<ExpiryAlert>` navigation property on `Stock.cs` were completely deleted, and the table was dropped from Neon via migration.

> **⚠️ Flag**: All EF Core entity classes have **zero** data-annotation validation attributes. Validation is enforced via Fluent API CHECK constraints and ViewModel-level annotations only.

### 3.2 ViewModels (Models/)

| ViewModel | Has Validation Attributes? | Key Attributes |
|---|---|---|
| **RegisterViewModel** | ✅ Yes | Required, StringLength, EmailAddress, Phone, Compare, Range(bool) |
| **ContactViewModel** | ✅ Yes | Required, EmailAddress, StringLength(1000, min 10) |
| **MedicineFormViewModel** | ✅ Yes | Required, StringLength, Range(0.01-100000), RegularExpression for Unit |
| **SideEffectFormViewModel** | ✅ Yes | Required, StringLength(150, min 2) |
| **CategoryFormViewModel** | ✅ Yes | Required, StringLength(100, min 2), RegularExpression (letters only) |
| **ProductTypeFormViewModel** | ✅ Yes | Required, StringLength(50, min 2), RegularExpression |
| **AdminProfileFormViewModel** | ✅ Yes | Required, StringLength(150, min 2), Phone |
| **UserProfileViewModel** | ✅ Yes | Required, EmailAddress, Phone |
| **LoginModel.InputModel** (Identity) | ✅ Yes | Required, EmailAddress, DataType(Password) |
| **RegisterModel.InputModel** (Identity) | ✅ Yes | Required, StringLength, EmailAddress, Phone, Compare, Range(bool) |
| **ErrorViewModel** | ⚠️ No | Only has `RequestId` (string?) |
| **CartItemViewModel** | ⚠️ No | Display-only ViewModel |
| **CheckoutViewModel** | ⚠️ No | Display-only ViewModel |
| **CheckoutLineItemViewModel** | ⚠️ No | Display-only ViewModel |
| **MedicineViewModel** | ⚠️ No | Display-only ViewModel |
| **OrderConfirmationViewModel** | ⚠️ No | Display-only ViewModel |
| **AdminOrderDetailViewModel** | ⚠️ No | Display-only ViewModel |
| **AdminOrderListViewModel** | ⚠️ No | Display-only ViewModel |
| **AdminFlaggedOrdersListViewModel** | ⚠️ No | Display-only ViewModel |
| **AdminContactMessageListViewModel** | ⚠️ No | Display-only ViewModel |
| **AdminProfileViewModel** | ⚠️ No | Display-only ViewModel |
| **AdminMedicinesPageViewModel** | ⚠️ No | Display-only ViewModel |
| **AdminMedicineFormPageViewModel** | ⚠️ No | Wrapper — delegates to MedicineFormViewModel |
| All FilterOption ViewModels | ⚠️ No | Display-only ViewModels |

---

## 4. Business Logic (Services Layer)

### 4.1 Service Classes

#### CartService (`ICartService`)
| Method | Description |
|---|---|
| `GetCartAsync(userId)` | Returns cart items for user, releases expired items first, computes expiry warnings |
| `AddToCartAsync(userId, medicineId, quantity)` | Adds item to cart, deducts stock; blocks if expired or critical expiry (≤7 days) or insufficient stock; returns `OkWithWarning(...)` with `WarningMessage` for warning-tier items (>7–30 days); uses DB transaction |
| `UpdateQuantityAsync(userId, cartItemId, newQuantity)` | Updates cart quantity, adjusts stock delta; uses DB transaction |
| `RemoveItemAsync(userId, cartItemId)` | Removes cart item, restores stock; uses DB transaction |
| `GetCartItemCountAsync(userId)` | Returns sum of quantities in user's cart |
| `ReleaseExpiredCartItemsAsync(userId?)` | Removes cart items older than 3 days (`CartExpiry = TimeSpan.FromDays(3)`), returns stock |

> **Cart Warning Notification**: `CartOperationResult` includes `WarningMessage (string?)`. `AddToCartAsync` returns `OkWithWarning(...)` with an expiry warning message for warning-tier items; `CartController.Add` forwards it as `warningMessage` in JSON; `cart-handler.js` reads it and shows a toast.

#### OrderService (`IOrderService`)
| Method | Description |
|---|---|
| `PlaceOrderAsync(PlaceOrderRequest)` | Creates Order + OrderItems + Prescription (if Rx) + Payment, clears cart; uses DB transaction |

#### CloudinaryImageService (`IImageUploadService`)
| Method | Description |
|---|---|
| `UploadMedicineImageAsync(IFormFile)` | Uploads image to Cloudinary (folder: `medicart/medicines`), returns secure URL |

#### EmailService (`IEmailService`)
| Method | Description |
|---|---|
| `SendOtpEmailAsync(toEmail, otpCode)` | Sends OTP via Gmail SMTP using MailKit |

#### CartExpiryBackgroundService (`BackgroundService`)
| Method | Description |
|---|---|
| `ExecuteAsync(CancellationToken)` | Runs every 1 hour (`CheckInterval = TimeSpan.FromHours(1)`), calls `ReleaseExpiredCartItemsAsync(null)` for all users |

#### SensitivityFlagHelper (static utility)
| Method | Description |
|---|---|
| `IsOverThreshold(sensitivityLevel, quantity)` | Returns true if quantity exceeds the tier's threshold |
| `GetThreshold(sensitivityLevel)` | Returns the threshold int for a given tier |

#### StockExpiryHelper (static utility — `Services/StockExpiryHelper.cs`)
Single source of truth for all stock and expiry thresholds and state checks across the application.
> **Rule**: Every controller, service, and ViewModel must use this helper — never hard-code raw magic numbers anywhere else.

- **Thresholds**:
  - `LowStockThreshold = 10`
  - `WarningExpiryDays = 30`
  - `CriticalExpiryDays = 7`
- **Methods & Logic**:
  - `IsOutOfStock(int quantity) => quantity == 0`
  - `IsLowStock(int quantity) => quantity <= LowStockThreshold` (<= 10 units, inclusive of exactly 10)
  - `IsExpired(int daysUntilExpiry) => daysUntilExpiry < 0`
  - `IsCriticalExpiry(int daysUntilExpiry) => daysUntilExpiry >= 0 && daysUntilExpiry <= CriticalExpiryDays` (0–7 days)
  - `IsWarningExpiry(int daysUntilExpiry) => daysUntilExpiry > CriticalExpiryDays && daysUntilExpiry <= WarningExpiryDays` (>7–30 days)
  - `IsBlockedFromCart(int daysUntilExpiry) => daysUntilExpiry <= CriticalExpiryDays` (covers negative/expired and critical 0–7 days)
  - `DaysUntilExpiry(DateOnly expiryDate) => expiryDate.DayNumber - today.DayNumber`
  - `GetStockBadge(int quantity)`: returns `("Out of stock", "danger")` for qty 0, `("Low stock", "warning")` for qty <= 10, or `(null, null)`
  - `GetExpiryBadge(int daysUntilExpiry)`: returns `("Expired", "danger")` for < 0, `("Critical", "danger")` for <= 7, `($"Expires in {daysUntilExpiry} days", "warning")` for <= 30, or `(null, null)`

### 4.2 Flagged-Orders Sensitivity Tiers (SensitivityFlagHelper)

| Tier | Threshold (units) | Constant Name |
|---|---|---|
| **High** | ≥ 5 | `HighThreshold = 5` |
| **Mid** | ≥ 15 | `MidThreshold = 15` |
| **Low** | ≥ 30 | `LowThreshold = 30` |

### 4.3 Stock & Inventory Health Thresholds
- **Low stock threshold**: `<= 10 units`, inclusive of exactly 10 (`StockExpiryHelper.LowStockThreshold = 10`).
- **Out of stock**: `Quantity == 0` (`StockExpiryHelper.IsOutOfStock(quantity)`).

### 4.4 Expiry Alert Tiers & State Checks
Defined server-side in `Services/StockExpiryHelper.cs`:
- **Expired**: `< 0 days` until expiry (already expired) — blocked from cart and checkout. Badge: `--color-danger` ("Expired").
- **Critical**: `0–7 days` until expiry — blocked from cart and checkout (`IsBlockedFromCart` / `IsBlockedFromCheckout`). Badge: `--color-danger` ("Critical").
- **Warning**: `> 7–30 days` until expiry — customer can purchase and checkout; shows warning badge and toast on add-to-cart (`WarningMessage`). Badge: `--color-warning` ("Expires in {X} days" with real day count).
- **Normal**: `> 30 days` until expiry — normal inventory, no expiry badge shown.

> **Deleted Entity Note**: The `ExpiryAlert` entity and table have been deleted from the project and must not be referenced. All expiry logic is evaluated on the fly via `StockExpiryHelper`.

### 4.5 ViewModel Properties for Stock & Expiry
All affected ViewModels compute or bind directly via `StockExpiryHelper`. `IsNearExpiryWindow` and `IsExpiringSoon` were **deleted** — do not use.

Current correct property sets:
- **`MedicineViewModel`** (Browse medicines & modal):
  `IsExpired`, `IsCriticalExpiry`, `IsWarningExpiry`, `IsLowStock`, `IsOutOfStock`, `IsBlockedFromCart`, `StockStatus`, `StockCssClass`.
- **`CartItemViewModel`** (Customer cart):
  `IsExpired`, `IsCriticalExpiry`, `IsWarningExpiry`, `IsLowStock`, `IsOutOfStock`, `IsBlockedFromCheckout`, `StockStatus`, `StockCssClass`.
- **`AdminStockExpiryRowViewModel`** (Admin stock & expiry table):
  `IsExpired`, `IsCriticalExpiry`, `IsWarningExpiry`, `IsLowStock`, `IsOutOfStock`, `StockBadge`/`StockSeverity`, `ExpiryBadge`/`ExpirySeverity`.
- **`MedicineListRowViewModel`** (Admin medicines table):
  `IsExpired`, `IsCriticalExpiry`, `IsWarningExpiry`, `IsLowStock`, `IsOutOfStock`, `IsBlockedFromCart`, `StockStatus`, `StockCssClass`.

### 4.6 Badge Rendering Rules
- **Independent, Stacked Badges**: Expiry and stock badges render as two independent, stacked badges when both apply to the same row — never merge them into a single combined badge. This applies identically across:
  - Admin Medicines table (`Views/Admin/Medicines.cshtml`)
  - Admin Stock & Expiry page (`Views/Admin/StockExpiry.cshtml`)
  - Browse Medicines product cards (`Views/Medicines/Index.cshtml`)
  - Cart page rows (`Views/Cart/Index.cshtml`)
- **Cart Badge Priority Order**:
  When evaluating or displaying badges top-to-bottom in summary/card contexts:
  `Expired → Critical → Warning → Out of stock → Low stock → Rx required → In stock`.
  Warning badge shows the real day count (e.g., "Expires in 18 days"), not a static string.
  Checkout blocks strictly on `Expired`, `Critical`, or `Out of stock` only — `Warning` tier does not block checkout.

### 4.7 DB Transaction Usage

| Location | Transaction? | Details |
|---|---|---|
| `CartService.AddToCartAsync` | ✅ Yes | `BeginTransactionAsync` → SaveChanges → Commit (rollback on catch) |
| `CartService.UpdateQuantityAsync` | ✅ Yes | Same pattern |
| `CartService.RemoveItemAsync` | ✅ Yes | Same pattern |
| `OrderService.PlaceOrderAsync` | ✅ Yes | `BeginTransactionAsync` → creates Order, OrderItems, Prescription, Payment, clears CartItems → Commit (rollback on catch) |
| `CartService.ReleaseExpiredCartItemsAsync` | ❌ No | Single `SaveChangesAsync`, no explicit transaction |
| All AdminController write actions | ❌ No | Single `SaveChangesAsync` calls, no explicit transactions |
| All AdminOrdersController status changes | ❌ No | Single `SaveChangesAsync` calls |

**Note**: `OrderService.PlaceOrderAsync` does **not re-deduct stock** at order placement time. Stock was already deducted when items were added to cart. The order flow is: add to cart (deducts stock) → checkout → place order (clears cart). No second stock deduction occurs during order creation.

---

## 5. Features Actually Implemented vs Not Yet Built

### Guest Features

| Feature | Status | Notes |
|---|---|---|
| About Us page | ✅ Done | `Home/About` view exists |
| Contact Us form | ✅ Done | GET + POST with validation, saves ContactMessage, Subject folded into Message text |
| Register | ✅ Done | Identity Razor Page + custom MVC view, auto-assigns Customer role |

### Customer Features

| Feature | Status | Notes |
|---|---|---|
| Login/Register | ✅ Done | Identity Razor Pages with role-based redirect |
| Browse/search/filter medicines | ✅ Done | `Medicines/Index` with category, subcategory, product type filters (JS client-side filtering) |
| Medicine detail | ✅ Done | Modal/detail panel rendered client-side from medicine data in `Medicines/Index` |
| Cart | ✅ Done | Add, update quantity, remove, expiry warnings, 3-day auto-expiry background service |
| Checkout (address + payment + prescription upload) | ✅ Done | Division/city selection, phone, address, payment method (Cash on delivery / bKash with OTP / Card), prescription file upload |
| Order tracking | ✅ Done | `Confirmation/Index` and `Orders/Detail` show 4-stage tracker (Pending → Processing → Shipped → Delivered, or Rejected) |
| Profile + order history | ✅ Done | `UserProfile/Index` shows edit profile + order history list + sent messages list |

### Admin Features

| Feature | Status | Notes |
|---|---|---|
| Login | ✅ Done | Same Identity login, role-based redirect to Dashboard |
| Manage medicines | ✅ Done | List, add, edit, delete with image upload, search/filter |
| Manage categories | ✅ Done | CRUD for categories + subcategories |
| Manage product types | ✅ Done | CRUD for product types |
| Review & approve/reject/ship/deliver orders | ✅ Done | Full order lifecycle in `AdminOrdersController` with AuditLog entries |
| View prescription images | ✅ Done | `PrescriptionImageUrl` displayed in `OrderDetail` view |
| Flagged orders | ✅ Done | `FlaggedOrders` action with per-medicine tier breakdown, filterable by tier |
| Low-stock alerts & inventory health | ✅ Done | Fully implemented via `StockExpiryHelper.LowStockThreshold` (<= 10 units) across Admin Dashboard low-stock widget, Admin Stock & Expiry page, and Medicines table |
| Stock & expiry management | ✅ Done | Dedicated page at `Views/Admin/StockExpiry.cshtml`, action `AdminController.StockExpiry()`, wired to `_db.Stocks`, featuring four filter tabs (All items, Warning, Critical & Expired, Low stock) and independent stacked badges per row |
| Audit log | ✅ Done | Dedicated searchable audit log page at `Views/Admin/AuditLog.cshtml`, action `AdminController.AuditLog()` with search, action filtering, date filtering, and pagination |
| Contact messages inbox | ✅ Done | `Admin/ContactMessages` with Unread/Read/All filter, mark-as-read functionality |
| Dashboard | ✅ Done | Fully implemented at `Views/Admin/Dashboard.cshtml`, action `AdminController.Dashboard()` — includes low stock widget (`Stock.Quantity <= 10` via `StockExpiryHelper.LowStockThreshold`) and expiry alerts widget (warning + critical tiers) |

---

## 6. Validation & Error Handling Inventory

### 6.1 Client-Side Validation

| Location | Mechanism |
|---|---|
| Identity Login page (`Areas/Identity/Pages/Account/Login.cshtml`) | jQuery Validation Unobtrusive (`_ValidationScriptsPartial`) + custom `login.js` |
| Identity Register page (`Areas/Identity/Pages/Account/Register.cshtml`) | jQuery Validation Unobtrusive + custom `register.js` |
| MVC Register view (`Views/Account/Register.cshtml`) | jQuery Validation Unobtrusive + custom `register.js` |
| Admin AddMedicine form (`Views/Admin/AddMedicine.cshtml`) | jQuery Validation Unobtrusive + custom `admin-medicine-form.js` |
| Admin Categories page (`Views/Admin/Categories.cshtml`) | Custom `admin-categories.js` |
| Cart page (`Views/Cart/Index.cshtml`) | Custom `cart.js` |
| Checkout page (`Views/Checkout/Index.cshtml`) | Custom `checkout.js` |
| Medicines browse page (`Views/Medicines/Index.cshtml`) | Custom `medicine.js` (filtering/search) |
| Home page (`Views/Home/Index.cshtml`) | Custom `home.js` |
| Admin Medicines page (`Views/Admin/Medicines.cshtml`) | Custom `admin-medicines.js` |

### 6.2 Server-Side try-catch Coverage

| Controller/Action | Has try-catch? | What is caught |
|---|---|---|
| `CheckoutController.SendOtp` | ✅ Yes | Catches email send failure → returns 500 JSON |
| `CheckoutController.PlaceOrder` | ✅ Yes | Catches prescription Cloudinary upload failure → returns 500 JSON |
| `AdminController.AddMedicine` (POST) | ✅ Yes | Catches Cloudinary upload `InvalidOperationException` → adds ModelState error |
| `AdminController.EditMedicine` (POST) | ✅ Yes | Catches Cloudinary upload `InvalidOperationException` → adds ModelState error |
| `CartService.AddToCartAsync` | ✅ Yes | Transaction catch → rollback + rethrow |
| `CartService.UpdateQuantityAsync` | ✅ Yes | Transaction catch → rollback + rethrow |
| `CartService.RemoveItemAsync` | ✅ Yes | Transaction catch → rollback + rethrow |
| `OrderService.PlaceOrderAsync` | ✅ Yes | Transaction catch → rollback + rethrow |
| `CartExpiryBackgroundService.ExecuteAsync` | ✅ Yes | Catches and logs errors, continues running |
| `CloudinaryImageService.UploadMedicineImageAsync` | ✅ Yes | Catches, logs, rethrows |

### 6.3 Actions with NO try-catch Around DB/File Operations

⚠️ The following controller actions perform DB writes with **no try-catch**:
- `HomeController.Contact` (POST) — `SaveChangesAsync` with no error handling
- `AdminController.CreateCategory` — `SaveChangesAsync` with no error handling
- `AdminController.EditCategory` — `SaveChangesAsync` with no error handling
- `AdminController.DeleteCategory` — `SaveChangesAsync` with no error handling
- `AdminController.DeleteSubCategory` — `SaveChangesAsync` with no error handling
- `AdminController.CreateProductType` — `SaveChangesAsync` with no error handling
- `AdminController.EditProductType` — `SaveChangesAsync` with no error handling
- `AdminController.DeleteProductType` — `SaveChangesAsync` with no error handling
- `AdminController.DeleteMedicine` — `SaveChangesAsync` with no error handling
- `AdminController.MarkMessageAsRead` — `SaveChangesAsync` with no error handling
- `AdminController.UpdateProfile` — `_userManager.UpdateAsync` with no error handling (but does check `result.Succeeded`)
- `AdminOrdersController.Approve` — `SaveChangesAsync` with no error handling
- `AdminOrdersController.Reject` — `SaveChangesAsync` with no error handling
- `AdminOrdersController.MarkShipped` — `SaveChangesAsync` with no error handling
- `AdminOrdersController.MarkDelivered` — `SaveChangesAsync` with no error handling
- `UserProfileController.Index` (POST) — `_userManager.UpdateAsync` (checks result, but no try-catch)
- `CheckoutController.VerifyOtp` — `SaveChangesAsync` with no error handling
- `CartService.ReleaseExpiredCartItemsAsync` — `SaveChangesAsync` with no error handling

### 6.4 User-Facing Error Messages / Pages

| Location | Type | Message/Page |
|---|---|---|
| `Home/Error` view | Error page | Shows `ErrorViewModel.RequestId` (production: `/Home/Error` via `UseExceptionHandler`) |
| `Admin/ComingSoon` view | Placeholder page | Generic fallback view (Dashboard, StockExpiry, and AuditLog are fully implemented) |
| TempData `["CategoryError"]` | Flash message | Various category CRUD errors in AdminController |
| TempData `["CategorySuccess"]` | Flash message | Various category CRUD successes |
| TempData `["ProductTypeError"]` / `["ProductTypeSuccess"]` | Flash message | Product type CRUD |
| TempData `["MedicineError"]` / `["MedicineSuccess"]` | Flash message | Medicine CRUD |
| TempData `["MessageError"]` | Flash message | Contact message not found |
| TempData `["OrderError"]` / `["OrderSuccess"]` | Flash message | Order status change errors/successes |
| TempData `["ProfileError"]` / `["ProfileSuccess"]` | Flash message | Admin profile update |
| TempData `["ProfileSuccess"]` | Flash message | Customer profile update |
| TempData `["ContactSuccess"]` | Flash message | Contact form submission |
| ModelState errors | Inline | Login, Register, AddMedicine, EditMedicine, Contact, UserProfile forms |
| JSON `{ error: "..." }` | API response | Cart operations, Checkout operations |

---

## 7. File Upload (Prescription Images)

### 7.1 Storage
- Uploaded to **Cloudinary** (cloud-hosted), not local filesystem
- Cloudinary folder: `medicart/medicines`
- Stored URL: `Prescription.ImageUrl` (string) in DB
- Same service (`IImageUploadService`) used for both medicine images and prescription images

### 7.2 Server-Side Validation (Medicine Images Only)

In `AdminController.IsValidImageFile()` (lines 1242-1259):
```csharp
var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
const long maxBytes = 5 * 1024 * 1024; // 5 MB
return allowedTypes.Contains(file.ContentType) && file.Length <= maxBytes;
```

### 7.3 Prescription Upload Validation
- **⚠️ No explicit file type/size validation** on prescription files in `CheckoutController.PlaceOrder`
- The prescription file is passed directly to `_imageService.UploadMedicineImageAsync(prescriptionFile)` without calling `IsValidImageFile`
- Cloudinary may impose its own limits, but no server-side validation is explicitly coded for prescriptions
- Only check: `prescriptionFile != null && prescriptionFile.Length > 0`

### 7.4 Max File Size
- **Medicine images**: 5 MB (enforced by `IsValidImageFile`)
- **Prescription images**: No explicit limit in code (Cloudinary default applies)
- **No global `RequestSizeLimit` or `Kestrel.Limits.MaxRequestBodySize`** configured in Program.cs

---

## 8. Database & Config

### 8.1 DbSet Entities in ApplicationDbContext

1. `DbSet<Division> Divisions`
2. `DbSet<City> Cities`
3. `DbSet<Category> Categories`
4. `DbSet<SubCategory> SubCategories`
5. `DbSet<ProductType> ProductTypes`
6. `DbSet<Medicine> Medicines`
7. `DbSet<SideEffect> SideEffects`
8. `DbSet<Stock> Stocks`
9. `DbSet<CartItem> CartItems`
10. `DbSet<Order> Orders`
11. `DbSet<OrderItem> OrderItems`
12. `DbSet<Prescription> Prescriptions`
13. `DbSet<Notification> Notifications`
14. `DbSet<ContactMessage> ContactMessages`
15. `DbSet<AuditLog> AuditLogs`
16. `DbSet<OtpCode> OtpCodes`
17. `DbSet<Payment> Payments`

> **Note**: `DbSet<ExpiryAlert>` was completely deleted (`Data/ExpiryAlert.cs`, check constraints, navigation properties on `Stock.cs`, and DB table dropped via migration). This entity does not exist and must not be referenced.

Plus Identity tables inherited from `IdentityDbContext<ApplicationUser>`.

### 8.2 Connection String Configuration
- **`appsettings.json`**: `DefaultConnection` is set to `"SET_VIA_USER_SECRETS"` (the real PostgreSQL/Neon connection string lives only in user secrets)
- **User Secrets** (UserSecretsId: `aspnet-MediCart.Web-f12e498e-2a48-4b73-b33a-bdb02114f872`): Supplies the real Neon PostgreSQL connection string
- **Program.cs line 8**: `builder.Configuration.GetConnectionString("DefaultConnection")` — reads from configuration hierarchy (user-secrets overrides appsettings.json)
- **Cloudinary credentials**: Read from `Configuration["Cloudinary:CloudName"]`, `["Cloudinary:ApiKey"]`, `["Cloudinary:ApiSecret"]` — expected in user-secrets or App Service config
- **Email credentials**: Read from `Configuration["Email:From"]`, `["Email:AppPassword"]` — expected in user-secrets or App Service config

### 8.3 NuGet Packages (from .csproj)

| Package | Version | Purpose |
|---|---|---|
| `CloudinaryDotNet` | 1.29.3 | Image upload to Cloudinary |
| `MailKit` | 4.17.0 | SMTP email sending (OTP) |
| `Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore` | 10.0.11 | Developer exception page for EF |
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | 10.0.11 | Identity with EF Core |
| `Microsoft.AspNetCore.Identity.UI` | 10.0.11 | Default Identity UI (scaffolded) |
| `Microsoft.EntityFrameworkCore.Design` | 10.0.11 | EF Core design-time tools |
| `Microsoft.EntityFrameworkCore.Tools` | 10.0.11 | EF Core CLI tools |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | 10.0.3 | PostgreSQL provider for EF Core |

---

## 9. Recent Changes Flag

Last 15 commits (from `git log --oneline -15`):

| Hash | Message | Key Files Touched |
|---|---|---|
| `dba6c85` | Merge pull request #104 from meent1612/issue-103 | — |
| `5671016` | Wired up everything to make the filter and contact work | AdminController, AdminOrdersController, HomeController, UserProfileController, AdminContactMessageListVM, UserProfileVM, ContactMessages.cshtml, FlaggedOrders.cshtml, UserProfile/Index.cshtml, admin-messages.css, userprofile.css, seed.sql (12 files) |
| `ebb187a` | Add TierFilter to AdminFlaggedOrdersListViewModel | AdminFlaggedOrderViewModel.cs |
| `2c76050` | Migration reset: add ContactMessage.UserId | Migration files, ModelSnapshot |
| `8631c1f` | Configure ContactMessage-to-User relationship | ApplicationDbContext.cs |
| `1de9090` | Add optional UserId FK to ContactMessage | ContactMessage.cs |
| `98f28be` | Merge pull request #102 from meent1612/issue-101 | — |
| `acab8f1` | Users can send message to the Admin | AdminController, HomeController, AdminContactMessageListVM, ContactMessages.cshtml, admin-messages.css (5 files) |
| `134c154` | Merge pull request #100 from meent1612/issue-99 | — |
| `405940f` | feat(admin-orders): add styles for flagged items list | admin-orders.css |
| `5646f59` | feat(admin-orders): show sensitivity flag breakdown on order detail page | AdminOrders/OrderDetail.cshtml |
| `94206e6` | feat(admin-orders): add flagged orders list view | AdminOrders/FlaggedOrders.cshtml |
| `104c9bf` | feat(admin-orders): implement FlaggedOrders action with per-medicine tier breakdown | AdminOrdersController.cs |
| `ca1fa13` | feat(admin-orders): add FlaggedItems to order detail view model | AdminOrderDetailViewModel.cs |
| `d557b5d` | feat(admin-orders): add view models for flagged orders list | AdminFlaggedOrderViewModel.cs |

**Recent development focus**: Flagged orders feature (per-medicine sensitivity breakdown), contact messages inbox (admin side + user-submitted messages with optional UserId linking), tier filtering for flagged orders.

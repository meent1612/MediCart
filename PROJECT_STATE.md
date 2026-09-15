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
| `AdminController` | **COMMENTED OUT** `// [Authorize(Roles = "Admin")]` (line 10) | ⚠️ **Currently anyone can access all /Admin/* routes** |
| `AdminOrdersController` | **COMMENTED OUT** `// [Authorize(Roles = "Admin")]` (line 11) | ⚠️ **Currently anyone can access all /AdminOrders/* routes** |
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

### 2.8 AdminController (⚠️ `[Authorize]` COMMENTED OUT)

| Action | Verb | Route | Parameters | Return | Mutates? |
|---|---|---|---|---|---|
| `Profile` | GET | `/Admin/Profile` | — | View(AdminProfileViewModel) | No |
| `UpdateProfile` | POST | `/Admin/UpdateProfile` | AdminProfileFormViewModel | Redirect | **Yes** — updates admin user |
| `Medicines` | GET | `/Admin/Medicines` | search?, categoryId?, subCategoryId?, productTypeId? | View(AdminMedicinesPageViewModel) | No |
| `Dashboard` | GET | `/Admin/Dashboard` | — | View("ComingSoon") | No |
| `StockExpiry` | GET | `/Admin/StockExpiry` | — | View("ComingSoon") | No |
| `AuditLog` | GET | `/Admin/AuditLog` | — | View("ComingSoon") | No |
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

### 2.9 AdminOrdersController (⚠️ `[Authorize]` COMMENTED OUT)

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
| **ExpiryAlert** | `Id` (int), `StockId` (int), `MedicineId` (int), `AlertLevel` (string), `AlertDate` (DateOnly), `IsResolved` (bool) | ⚠️ None (CHECK constraint via Fluent API for AlertLevel) |
| **Notification** | `Id` (int), `UserId` (string), `Message` (string), `IsRead` (bool), `CreatedAt` (DateTime) | ⚠️ None |
| **ContactMessage** | `Id` (int), `Name` (string), `Email` (string), `Message` (string), `IsRead` (bool), `CreatedAt` (DateTime), `UserId` (string?) | ⚠️ None |
| **AuditLog** | `Id` (int), `AdminId` (string), `Action` (string), `TableName` (string?), `RecordId` (int?), `CreatedAt` (DateTime) | ⚠️ None |
| **OtpCode** | `Id` (int), `Email` (string), `Code` (string), `ExpiresAt` (DateTime), `IsUsed` (bool), `CreatedAt` (DateTime) | ⚠️ None |

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
| `AddToCartAsync(userId, medicineId, quantity)` | Adds item to cart, deducts stock; blocks if critical expiry (≤7 days) or insufficient stock; uses DB transaction |
| `UpdateQuantityAsync(userId, cartItemId, newQuantity)` | Updates cart quantity, adjusts stock delta; uses DB transaction |
| `RemoveItemAsync(userId, cartItemId)` | Removes cart item, restores stock; uses DB transaction |
| `GetCartItemCountAsync(userId)` | Returns sum of quantities in user's cart |
| `ReleaseExpiredCartItemsAsync(userId?)` | Removes cart items older than 3 days (`CartExpiry = TimeSpan.FromDays(3)`), returns stock |

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

### 4.2 Flagged-Orders Sensitivity Tiers (SensitivityFlagHelper)

| Tier | Threshold (units) | Constant Name |
|---|---|---|
| **High** | ≥ 5 | `HighThreshold = 5` |
| **Mid** | ≥ 15 | `MidThreshold = 15` |
| **Low** | ≥ 30 | `LowThreshold = 30` |

### 4.3 Low-Stock Alert Threshold

Defined in `MedicineListRowViewModel` (AdminMedicineListViewModel.cs line 17):
- `IsLowStock => StockQuantity <= 10`

This is a **computed property on the ViewModel** — there is no separate low-stock service or background alert. The admin medicines list page highlights medicines with ≤10 units in stock.

### 4.4 Expiry Alert Tiers

**CartService (customer-facing):**
- `CriticalExpiryDays = 7` — blocks add-to-cart, shown as critical warning
- `WarningExpiryDays = 30` — shown as soft warning in cart

**AdminMedicineListViewModel (admin-facing):**
- `IsExpired => ExpiryDate < DateOnly.FromDateTime(DateTime.UtcNow)` — already expired
- `IsExpiringSoon => ExpiryDate <= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30))` — expiring within 30 days

**MedicineViewModel (customer browse page):**
- `IsExpired => ExpiryDate < today`
- `IsExpiringSoon => ExpiryDate within 90 days`

**ExpiryAlert entity** exists in DB schema (with `AlertLevel IN ('warning','critical')`) but **no service or controller action writes ExpiryAlert records**. The `Admin/StockExpiry` action returns `View("ComingSoon")`.

### 4.5 DB Transaction Usage

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
| Low-stock alerts | 🟡 Partially Done | Computed property `IsLowStock <= 10` on admin medicine list ViewModel; displayed in UI. **No separate alert/notification system** |
| Two-tier expiry alerts | 🟡 Partially Done | ExpiryAlert entity + DB table exist, CHECK constraint for 'warning'/'critical' is configured. Admin medicines list computes `IsExpired` and `IsExpiringSoon`. **But**: `Admin/StockExpiry` returns "ComingSoon" — no controller logic writes ExpiryAlert records, no dedicated alert page is functional |
| Audit log | 🟡 Partially Done | AuditLog entity exists and is **written to** on order status changes (Approve, Reject, Ship, Deliver) and message read. AuditLog entries are displayed on Admin Profile page (recent 8). **But**: `Admin/AuditLog` action returns "ComingSoon" — no dedicated searchable audit log page |
| Contact messages inbox | ✅ Done | `Admin/ContactMessages` with Unread/Read/All filter, mark-as-read functionality |
| Dashboard | 🔴 Not Started | `Admin/Dashboard` returns "ComingSoon" placeholder view |

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
| `Admin/ComingSoon` view | Placeholder page | Used by Dashboard, StockExpiry, AuditLog |
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
13. `DbSet<ExpiryAlert> ExpiryAlerts`
14. `DbSet<Notification> Notifications`
15. `DbSet<ContactMessage> ContactMessages`
16. `DbSet<AuditLog> AuditLogs`
17. `DbSet<OtpCode> OtpCodes`
18. `DbSet<Payment> Payments`

Plus Identity tables inherited from `IdentityDbContext<ApplicationUser>`.

### 8.2 Connection String Configuration
- **`appsettings.json`**: Contains a fallback SQLite connection string (`DataSource=app.db;Cache=Shared`)
- **User Secrets** (UserSecretsId: `aspnet-MediCart.Web-f12e498e-2a48-4b73-b33a-bdb02114f872`): Expected to override with Neon PostgreSQL connection string
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

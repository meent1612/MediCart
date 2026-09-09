using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MediCart.Web.Data;
using MediCart.Web.Models;
using MediCart.Web.Services;

namespace MediCart.Web.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CheckoutController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly IEmailService _emailService;
        private readonly IImageUploadService _imageService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;

        public CheckoutController(
            ICartService cartService,
            IOrderService orderService,
            IEmailService emailService,
            IImageUploadService imageService,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext db)
        {
            _cartService = cartService;
            _orderService = orderService;
            _emailService = emailService;
            _imageService = imageService;
            _userManager = userManager;
            _db = db;
        }

        // =====================================================================
        // GET /Checkout
        // Loads divisions + cities from DB and passes them as JSON to the view.
        // =====================================================================
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;
            var cartItems = await _cartService.GetCartAsync(userId);

            if (cartItems.Count == 0)
                return RedirectToAction("Index", "Cart");

            // Load all divisions with their cities in one query.
            var divisions = await _db.Divisions
                .Include(d => d.Cities)
                .OrderBy(d => d.Name)
                .ToListAsync();

            var divisionsJson = JsonSerializer.Serialize(
                divisions.Select(d => new CheckoutDivisionViewModel
                {
                    Id = d.Id,
                    Name = d.Name,
                    DeliveryCharge = d.DeliveryCharge,
                    Cities = d.Cities
                        .OrderBy(c => c.Name)
                        .Select(c => new CheckoutCityViewModel
                        {
                            Id = c.Id,
                            Name = c.Name
                        }).ToList()
                }),
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                });

            var model = new CheckoutViewModel
            {
                Items = cartItems.Select(ci => new CheckoutLineItemViewModel
                {
                    Id = ci.MedicineId,
                    Name = ci.Name,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.UnitPrice,
                    RequiresRx = ci.RequiresRx
                }).ToList(),
                DivisionsJson = divisionsJson
            };

            return View(model);
        }

        // =====================================================================
        // POST /Checkout/SendOtp
        // Generates a 5-digit OTP, saves it to DB, emails it to the customer.
        // =====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendOtp()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var email = user.Email!;

            // Mark any existing unused OTPs for this email as used
            // so old codes cannot be replayed.
            var oldOtps = await _db.OtpCodes
                .Where(o => o.Email == email && !o.IsUsed)
                .ToListAsync();

            foreach (var old in oldOtps)
                old.IsUsed = true;

            // Generate a 5-digit OTP.
            var code = new Random().Next(10000, 99999).ToString();

            _db.OtpCodes.Add(new OtpCode
            {
                Email = email,
                Code = code,
                ExpiresAt = DateTime.UtcNow.AddSeconds(30),
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            // Send the email — if this throws, the customer sees a generic error.
            try
            {
                await _emailService.SendOtpEmailAsync(email, code);
            }
            catch
            {
                return StatusCode(500, new { error = "Failed to send OTP email. Please try again." });
            }

            return Ok(new { message = "OTP sent." });
        }

        // =====================================================================
        // POST /Checkout/VerifyOtp
        // Checks the submitted OTP against the DB record.
        // =====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var email = user.Email!;

            var otp = await _db.OtpCodes
                .Where(o => o.Email == email
                         && o.Code == request.Code
                         && !o.IsUsed
                         && o.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

            if (otp == null)
                return BadRequest(new { error = "Invalid or expired OTP." });

            otp.IsUsed = true;
            await _db.SaveChangesAsync();

            return Ok(new { message = "OTP verified." });
        }

        // =====================================================================
        // POST /Checkout/PlaceOrder
        // Validates all fields, uploads prescription if needed, places order.
        // =====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(
            int divisionId,
            int cityId,
            string addressLine,
            string phone,
            string paymentMethod,
            IFormFile? prescriptionFile)
        {
            var userId = _userManager.GetUserId(User)!;

            // Server-side validation
            if (string.IsNullOrWhiteSpace(addressLine))
                return BadRequest(new { error = "Address is required." });

            if (string.IsNullOrWhiteSpace(phone) || phone.Length != 11)
                return BadRequest(new { error = "Phone number must be 11 digits." });

            if (divisionId <= 0 || cityId <= 0)
                return BadRequest(new { error = "Please select a valid division and city." });

            // Upload prescription to Cloudinary if provided.
            string? prescriptionImageUrl = null;
            if (prescriptionFile != null && prescriptionFile.Length > 0)
            {
                try
                {
                    prescriptionImageUrl = await _imageService.UploadMedicineImageAsync(prescriptionFile);
                }
                catch
                {
                    return StatusCode(500, new { error = "Prescription upload failed. Please try again." });
                }
            }

            var result = await _orderService.PlaceOrderAsync(new PlaceOrderRequest
            {
                UserId = userId,
                DivisionId = divisionId,
                CityId = cityId,
                AddressLine = addressLine,
                Phone = phone,
                PaymentMethod = paymentMethod,
                PrescriptionImageUrl = prescriptionImageUrl
            });

            if (!result.Success)
                return BadRequest(new { error = result.ErrorMessage });

            return Ok(new { orderId = result.OrderId });
        }
    }

    // Small request model for VerifyOtp — comes in as JSON body.
    public class VerifyOtpRequest
    {
        public string Code { get; set; } = string.Empty;
    }
}
using System.ComponentModel.DataAnnotations;

namespace MediCart.Web.Models
{
    // Backs Views/Account/Register.cshtml.
    // Server-side validation is the source of truth — the front-end JS
    // (wwwroot/js/register.js) only mirrors these rules for instant feedback.
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Enter your full name")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Enter your full name")]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter a phone number")]
        [Phone(ErrorMessage = "Enter a valid phone number")]
        [Display(Name = "Phone number")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter an email address")]
        [EmailAddress(ErrorMessage = "Enter a valid email address")]
        [Display(Name = "Email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter a password")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Use at least 8 characters")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm your password")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Passwords don't match")]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Range(typeof(bool), "true", "true", ErrorMessage = "You must agree to the terms to continue")]
        [Display(Name = "Agree to terms")]
        public bool AgreeToTerms { get; set; }
    }
}

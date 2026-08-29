using System.ComponentModel.DataAnnotations;

namespace MediCart.Web.Models
{
    public class UserProfileViewModel
    {
        [Required(ErrorMessage = "Enter your full name")]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = null!;

        [Display(Name = "Email address")]
        public string Email { get; set; } = null!;  // read-only, shown but not edited here

        [Phone(ErrorMessage = "Enter a valid phone number")]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; } = null!;
    }
}
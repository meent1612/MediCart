using System.ComponentModel.DataAnnotations;

namespace MediCart.Web.Models
{
    public class UserProfileViewModel
    {
        [Required(ErrorMessage = "Enter your full name")]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter an email address")]
        [EmailAddress(ErrorMessage = "Enter a valid email address")]
        [Display(Name = "Email address")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Enter a valid phone number")]
        [Display(Name = "Phone number")]
        public string? PhoneNumber { get; set; }

        public List<OrderHistoryItem> Orders { get; set; } = new();
    }

    public class OrderHistoryItem
    {
        public int RealOrderId { get; set; }        // actual DB Id — used for the "View" link
        public string OrderId { get; set; } = string.Empty;   // display code e.g. "MC-10005"
        public DateTime Date { get; set; }
        public int ItemCount { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
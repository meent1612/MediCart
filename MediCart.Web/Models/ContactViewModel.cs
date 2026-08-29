using System.ComponentModel.DataAnnotations;

namespace MediCart.Web.Models
{
    public class ContactViewModel
    {
        [Required(ErrorMessage = "Enter your name")]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Enter your email")]
        [EmailAddress(ErrorMessage = "Enter a valid email address")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Enter a subject")]
        public string Subject { get; set; } = null!;

        [Required(ErrorMessage = "Enter your message")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Message should be at least 10 characters")]
        public string Message { get; set; } = null!;
    }
}
using Microsoft.AspNetCore.Identity;

namespace MediCart.Web.Data
{
    // Extends the built-in IdentityUser (which already has Email, PhoneNumber,
    // PasswordHash, etc.) with the extra field our Register form collects.
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
    }
}

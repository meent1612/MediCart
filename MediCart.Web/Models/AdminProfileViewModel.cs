using System.ComponentModel.DataAnnotations;

namespace MediCart.Web.Models
{
    public class AdminActivityItemViewModel
    {
        public string Action { get; set; } = "";
        public string? TableName { get; set; }
        public int? RecordId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AdminProfileViewModel
    {
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public string Initials { get; set; } = "";

        // Read-only — comes from ASP.NET Identity roles (UserManager.GetRolesAsync).
        // Not editable here: nothing in the backend currently supports changing
        // an admin's role from this form.
        public string RoleDisplay { get; set; } = "Admin";

        public List<AdminActivityItemViewModel> RecentActivity { get; set; } = new();

        // TODO(backend): none of these have a column in ApplicationUser yet.
        // Left null on purpose so the view can show "Not available yet"
        // instead of faking data.
        public string? Bio { get; set; }
        public DateTime? JoinedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }

        // TODO(backend): Medicine/Order have no CreatedByAdminId /
        // ReviewedByAdminId column, so these can't be computed per-admin yet
        // even once AuditLog starts being written to.
        public int? OrdersReviewed { get; set; }
        public int? MedicinesAdded { get; set; }
        public int? FlagsResolved { get; set; }
    }

    public class AdminProfileFormViewModel
    {
        [Required(ErrorMessage = "Enter your full name")]
        [StringLength(150, MinimumLength = 2)]
        public string FullName { get; set; } = "";

        [Phone(ErrorMessage = "Enter a valid phone number")]
        public string? PhoneNumber { get; set; }
    }
}

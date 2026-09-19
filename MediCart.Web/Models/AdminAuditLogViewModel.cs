using System;
using System.Collections.Generic;

namespace MediCart.Web.Models
{
    public class AdminAuditLogViewModel
    {
        public string? ActionFilter { get; set; } = "All";
        public string? AdminIdFilter { get; set; } = "All";
        public int TotalCount { get; set; }

        public List<AdminUserOptionViewModel> AdminOptions { get; set; } = new();
        public List<AuditLogGroupViewModel> Groups { get; set; } = new();
    }

    public class AdminUserOptionViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class AuditLogGroupViewModel
    {
        public string DateLabel { get; set; } = string.Empty; // "Today", "Yesterday", "15 Sep 2026"
        public List<AuditLogRowViewModel> Entries { get; set; } = new();
    }

    public class AuditLogRowViewModel
    {
        public int Id { get; set; }
        public string Action { get; set; } = string.Empty;
        public string ActionType { get; set; } = "Other"; // Add, Edit, Delete, Security, View
        public string? TableName { get; set; }
        public int? RecordId { get; set; }
        public string AdminName { get; set; } = string.Empty;
        public string AdminEmail { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string TimeString { get; set; } = string.Empty;
    }
}

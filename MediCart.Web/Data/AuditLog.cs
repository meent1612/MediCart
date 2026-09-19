namespace MediCart.Web.Data
{
    public static class AuditActionTypes
    {
        // Generic categories
        public const string Add = "Add";
        public const string Edit = "Edit";
        public const string Delete = "Delete";
        public const string Security = "Security";
        public const string Other = "Other";

        // Specific order & message state actions
        public const string MarkedRead = "MarkedRead";
        public const string MarkedUnread = "MarkedUnread";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";
        public const string MarkedShipped = "MarkedShipped";
        public const string MarkedDelivered = "MarkedDelivered";
        public const string Cancelled = "Cancelled";
    }

    public class AuditLog
    {
        public int Id { get; set; }
        public string AdminId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? ActionType { get; set; }
        public string? TableName { get; set; }
        public int? RecordId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ApplicationUser Admin { get; set; } = null!;
    }
}
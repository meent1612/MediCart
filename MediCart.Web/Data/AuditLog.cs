namespace MediCart.Web.Data
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string AdminId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? TableName { get; set; }
        public int? RecordId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ApplicationUser Admin { get; set; } = null!;
    }
}
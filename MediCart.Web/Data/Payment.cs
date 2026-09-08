namespace MediCart.Web.Data
{
    public class Payment
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Method { get; set; } = string.Empty;  // "Cash on delivery", "bKash", "Card"
        public string Status { get; set; } = "pending";     // "pending", "completed", "failed"
        public DateTime? PaidAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Order Order { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
    }
}
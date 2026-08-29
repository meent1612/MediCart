namespace MediCart.Web.Data
{
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int DivisionId { get; set; }
        public int CityId { get; set; }
        public string AddressLine { get; set; } = string.Empty;
        public decimal DeliveryCharge { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public bool IsFlagged { get; set; } = false;
        public string? RejectionReason { get; set; }
        public string? PaymentMethod { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ApplicationUser User { get; set; } = null!;
        public Division Division { get; set; } = null!;
        public City City { get; set; } = null!;
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public Prescription? Prescription { get; set; }
    }
}
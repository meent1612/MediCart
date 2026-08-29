namespace MediCart.Web.Data
{
    public class Prescription
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int OrderId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string Status { get; set; } = "pending";
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public ApplicationUser User { get; set; } = null!;
        public Order Order { get; set; } = null!;
    }
}
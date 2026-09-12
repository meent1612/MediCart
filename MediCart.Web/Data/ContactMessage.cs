namespace MediCart.Web.Data
{
    public class ContactMessage
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Null for guest submissions. Set when the submitter was logged in
        // at the time of submission — enables "My messages" on the
        // customer profile (Step 3 follow-up, #5).
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }
    }
}
namespace MediCart.Web.Models
{
    public class AdminContactMessageRowViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Message { get; set; } = "";
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AdminContactMessageListViewModel
    {
        public List<AdminContactMessageRowViewModel> Messages { get; set; } = new();
        public int UnreadCount { get; set; }
        public int TotalCount { get; set; }

        // "All" | "Unread" | "Read"
        public string StatusFilter { get; set; } = "Unread";
    }
}
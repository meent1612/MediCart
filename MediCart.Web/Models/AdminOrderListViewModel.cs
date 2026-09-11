namespace MediCart.Web.Models
{
    public class AdminOrderRowViewModel
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public int ItemCount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "";
        public bool IsFlagged { get; set; }
        public bool HasPrescription { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AdminOrderListViewModel
    {
        public List<AdminOrderRowViewModel> Orders { get; set; } = new();

        // "Active" | "All" | "Pending" | "Processing" | "Shipped" | "Delivered" | "Rejected"
        public string StatusFilter { get; set; } = "Active";

        public int TotalCount { get; set; }
    }
}
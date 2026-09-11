namespace MediCart.Web.Models
{
    public class AdminOrderItemRowViewModel
    {
        public string MedicineName { get; set; } = "";
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal => UnitPrice * Quantity;
    }

    public class AdminOrderDetailViewModel
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = "";
        public string Status { get; set; } = "";
        public bool IsFlagged { get; set; }
        public DateTime CreatedAt { get; set; }

        public string CustomerName { get; set; } = "";
        public string CustomerEmail { get; set; } = "";
        public string Phone { get; set; } = "";
        public string DeliveryAddress { get; set; } = "";

        public string? PaymentMethod { get; set; }
        public decimal DeliveryCharge { get; set; }
        public string? RejectionReason { get; set; }

        public List<AdminOrderItemRowViewModel> Items { get; set; } = new();

        // Every medicine+tier combination that crossed its threshold.
        // Empty unless IsFlagged is true.
        public List<FlaggedItemViewModel> FlaggedItems { get; set; } = new();

        // Informational only for now — Prescription.Status verification
        // workflow is not wired to Approve (Option 1, decided CP2 Step 2).
        public string? PrescriptionImageUrl { get; set; }
        public string? PrescriptionStatus { get; set; }

        public decimal Subtotal => Items.Sum(i => i.LineTotal);
        public decimal Total => Subtotal + DeliveryCharge;

        public bool CanApprove => Status == "Pending";
        public bool CanReject => Status == "Pending";
        public bool CanMarkShipped => Status == "Processing";
        public bool CanMarkDelivered => Status == "Shipped";
    }
}
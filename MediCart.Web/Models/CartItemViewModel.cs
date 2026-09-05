namespace MediCart.Web.Models
{
    public class CartItemViewModel
    {
        public int Id { get; set; }
        public int MedicineId { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";   // e.g. "Omeprazole 20mg · strip of 10"
        public string IconType { get; set; } = "tablet"; // "tablet" or "bottle" — used by existing CSS
        public string? ImageUrl { get; set; }
        public bool RequiresRx { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public int AvailableStock { get; set; }         // how many more the customer could add
        public DateTime AddedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // True when the medicine's stock expiry date is within 7 days (critical tier).
        // Customer cannot proceed to checkout if this is true.
        // Shown as a warning in the cart view.
        public bool IsCriticalExpiry { get; set; }

        // True when the medicine's stock expiry date is within 30 days (warning tier).
        // Customer can still buy, but a soft warning is shown.
        public bool IsWarningExpiry { get; set; }

        public decimal LineTotal => UnitPrice * Quantity;
    }
}
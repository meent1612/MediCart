namespace MediCart.Web.Models
{
    public class CartItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = ""; // e.g. "Omeprazole 20mg · strip of 10"
        public string IconType { get; set; } = "tablet"; // "tablet" or "bottle"
        public bool RequiresRx { get; set; }
        public string StockStatus { get; set; } = "In stock";
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal => UnitPrice * Quantity;
    }
}

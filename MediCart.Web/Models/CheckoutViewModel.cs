namespace MediCart.Web.Models
{
    public class CheckoutLineItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public bool RequiresRx { get; set; }
        public decimal LineTotal => UnitPrice * Quantity;
    }

    public class CheckoutDivisionViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public decimal DeliveryCharge { get; set; }
        public List<CheckoutCityViewModel> Cities { get; set; } = new();
    }

    public class CheckoutCityViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    public class CheckoutViewModel
    {
        public List<CheckoutLineItemViewModel> Items { get; set; } = new();

        // Serialized as JSON into the page so JS can populate dropdowns
        // without an extra AJAX call on first load.
        public string DivisionsJson { get; set; } = "[]";

        public decimal Subtotal => Items.Sum(i => i.LineTotal);
        public bool RequiresPrescription => Items.Any(i => i.RequiresRx);
        public string? RxItemName => Items.FirstOrDefault(i => i.RequiresRx)?.Name;
    }
}
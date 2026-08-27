using System.Collections.Generic;
using System.Linq;

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

    public class CheckoutViewModel
    {
        public List<CheckoutLineItemViewModel> Items { get; set; } = new();
        public string DefaultDivision { get; set; } = "Dhaka";
        public string DefaultCity { get; set; } = "Mirpur";

        public decimal Subtotal => Items.Sum(i => i.LineTotal);
        public bool RequiresPrescription => Items.Any(i => i.RequiresRx);
        public string? RxItemName => Items.FirstOrDefault(i => i.RequiresRx)?.Name;
    }
}

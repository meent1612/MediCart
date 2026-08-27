using System.Collections.Generic;
using System.Linq;

namespace MediCart.Web.Models
{
    public class OrderTrackingStage
    {
        public string Label { get; set; } = "";
        public string Timestamp { get; set; } = "";
        public bool IsDone { get; set; }
    }

    public class OrderConfirmationViewModel
    {
        public string OrderNumber { get; set; } = "";
        public string PlacedAt { get; set; } = "";
        public string StatusLabel { get; set; } = "";
        public List<OrderTrackingStage> Stages { get; set; } = new();

        public string CustomerName { get; set; } = "";
        public string DeliveryAddress { get; set; } = "";
        public string Phone { get; set; } = "";
        public string? AdminNote { get; set; }

        public List<CheckoutLineItemViewModel> Items { get; set; } = new();
        public decimal DeliveryCharge { get; set; }

        public decimal Subtotal => Items.Sum(i => i.LineTotal);
        public decimal Total => Subtotal + DeliveryCharge;
    }
}

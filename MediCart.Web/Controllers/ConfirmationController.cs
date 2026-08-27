using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MediCart.Web.Models;

namespace MediCart.Web.Controllers
{
    public class ConfirmationController : Controller
    {
        public IActionResult Index()
        {
            // TODO(backend): replace with the real placed order (by id/session)
            // once orders are persisted. Seeded to demo the tracking layout.
            var model = new OrderConfirmationViewModel
            {
                OrderNumber = "MC-10482",
                PlacedAt = "21 Aug 2026, 6:42 PM",
                StatusLabel = "Shipped",
                Stages = new List<OrderTrackingStage>
                {
                    new OrderTrackingStage { Label = "Pending", Timestamp = "21 Aug, 6:42 PM", IsDone = true },
                    new OrderTrackingStage { Label = "Processing", Timestamp = "21 Aug, 8:10 PM", IsDone = true },
                    new OrderTrackingStage { Label = "Shipped", Timestamp = "22 Aug, 10:05 AM", IsDone = true },
                    new OrderTrackingStage { Label = "Delivered", Timestamp = "Expected today", IsDone = false },
                },
                CustomerName = "Farhana Rahman",
                DeliveryAddress = "House 14, Road 6, Sector 10, Mirpur, Dhaka",
                Phone = "01712-XXXXXX",
                AdminNote = "Order approved after prescription verification for Seclo 20. No sensitivity flags raised on this order.",
                Items = new List<CheckoutLineItemViewModel>
                {
                    new CheckoutLineItemViewModel { Id = 1, Name = "Seclo 20", Quantity = 2, UnitPrice = 60, RequiresRx = true },
                    new CheckoutLineItemViewModel { Id = 2, Name = "Ambrox Syrup", Quantity = 1, UnitPrice = 85, RequiresRx = false },
                    new CheckoutLineItemViewModel { Id = 3, Name = "Napa Extra", Quantity = 3, UnitPrice = 30, RequiresRx = false },
                },
                DeliveryCharge = 60
            };

            return View(model);
        }
    }
}

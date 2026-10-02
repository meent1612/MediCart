using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using MediCart.Web.Data;

namespace MediCart.Web.Services.Ai.Tools
{
    public class CustomerOrderTrackingTool : IAiTool
    {
        private readonly ApplicationDbContext _db;

        public CustomerOrderTrackingTool(ApplicationDbContext db)
        {
            _db = db;
        }

        public string Name => "search_customer_orders";

        public string Description =>
            """
            Searches the authenticated customer's own MediCart orders.

            Use this tool when the customer asks about:
            - their order status
            - where their order is
            - their latest order
            - a specific order number
            - their order history
            - whether an order was rejected
            - why an order is currently rejected, when a rejection reason exists

            IMPORTANT:
            The tool automatically restricts results to the authenticated customer.
            Never ask the model to provide a user ID.
            Never return another customer's order.

            Order status is the current status stored in the database.
            The database stores the order creation time, but it does not store
            individual timestamps for every status transition.
            """;

        public JsonObject ParametersSchema =>
            new()
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["orderId"] = new JsonObject
                    {
                        ["type"] = "integer",
                        ["description"] =
                            "Specific order ID if the customer mentions an order number. Omit this when asking for the latest or recent orders."
                    },

                    ["latestOnly"] = new JsonObject
                    {
                        ["type"] = "boolean",
                        ["description"] =
                            "Set to true when the customer asks about their latest or most recent order."
                    },

                    ["limit"] = new JsonObject
                    {
                        ["type"] = "integer",
                        ["description"] =
                            "Maximum number of recent orders to return. Use a small value such as 5."
                    }
                },
                ["additionalProperties"] = false
            };

        public IReadOnlyCollection<string> AllowedRoles =>
            new[] { "Customer" };

        public async Task<string> ExecuteAsync(
            JsonObject arguments,
            AiCallContext context,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(context.UserId))
            {
                return JsonSerializer.Serialize(new
                {
                    success = false,
                    error = "The customer account could not be identified."
                });
            }

            int? orderId = GetInt(arguments, "orderId");
            bool latestOnly = GetBool(arguments, "latestOnly") ?? false;

            int limit = GetInt(arguments, "limit") ?? 5;
            limit = Math.Clamp(limit, 1, 10);

            IQueryable<Order> query = _db.Orders
                .AsNoTracking()
                .Where(o => o.UserId == context.UserId)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Medicine)
                .Include(o => o.Division)
                .Include(o => o.City);

            if (orderId.HasValue)
            {
                query = query.Where(o => o.Id == orderId.Value);
            }

            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Take(latestOnly ? 1 : limit)
                .Select(o => new
                {
                    orderId = o.Id,
                    orderNumber = $"MC-{10000 + o.Id}",
                    status = o.Status,
                    createdAt = o.CreatedAt,
                    totalAmount = o.TotalAmount,
                    deliveryCharge = o.DeliveryCharge,
                    address = o.AddressLine,
                    phone = o.Phone,
                    paymentMethod = o.PaymentMethod,
                    isFlagged = o.IsFlagged,
                    rejectionReason = o.RejectionReason,
                    division = o.Division.Name,
                    city = o.City.Name,

                    items = o.OrderItems
                        .OrderBy(oi => oi.Id)
                        .Select(oi => new
                        {
                            medicineName = oi.Medicine.Name,
                            quantity = oi.Quantity,
                            unitPrice = oi.UnitPrice,
                            lineTotal = oi.UnitPrice * oi.Quantity
                        })
                        .ToList()
                })
                .ToListAsync(ct);

            if (orders.Count == 0)
            {
                return JsonSerializer.Serialize(new
                {
                    success = true,
                    count = 0,
                    results = Array.Empty<object>()
                });
            }

            return JsonSerializer.Serialize(new
            {
                success = true,
                count = orders.Count,
                results = orders
            });
        }

        private static int? GetInt(JsonObject arguments, string name)
        {
            var node = arguments[name];

            if (node == null)
                return null;

            if (node is JsonValue value)
            {
                if (value.TryGetValue<int>(out var intValue))
                    return intValue;

                if (value.TryGetValue<string>(out var stringValue)
                    && int.TryParse(stringValue, out var parsed))
                {
                    return parsed;
                }
            }

            return null;
        }

        private static bool? GetBool(JsonObject arguments, string name)
        {
            var node = arguments[name];

            if (node == null)
                return null;

            if (node is JsonValue value)
            {
                if (value.TryGetValue<bool>(out var boolValue))
                    return boolValue;

                if (value.TryGetValue<string>(out var stringValue)
                    && bool.TryParse(stringValue, out var parsed))
                {
                    return parsed;
                }
            }

            return null;
        }
    }
}
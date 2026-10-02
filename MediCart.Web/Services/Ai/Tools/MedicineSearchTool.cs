using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using MediCart.Web.Data;

namespace MediCart.Web.Services.Ai.Tools
{
    public class MedicineSearchTool : IAiTool
    {
        private const int MaxResults = 20;

        private readonly ApplicationDbContext _db;

        public MedicineSearchTool(ApplicationDbContext db)
        {
            _db = db;
        }

        public string Name => "search_medicines";

        public string Description =>
            "Search the MediCart medicine catalogue using real database data. " +
            "Use this when a customer asks to find medicines by name, generic name, " +
            "category, subcategory, product type, price, stock availability, " +
            "prescription requirement, or expiry period. " +
            "Never invent medicine names, prices, stock quantities, categories, or expiry dates.";

        public JsonObject ParametersSchema => new()
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["search"] = new JsonObject
                {
                    ["type"] = "string",
                    ["description"] =
                        "Medicine name or generic name to search for. " +
                        "Use a concise search term, not the whole customer message."
                },

                ["maxPrice"] = new JsonObject
                {
                    ["type"] = "number",
                    ["description"] =
                        "Maximum medicine price in Bangladeshi taka."
                },

                ["minPrice"] = new JsonObject
                {
                    ["type"] = "number",
                    ["description"] =
                        "Minimum medicine price in Bangladeshi taka."
                },

                ["inStock"] = new JsonObject
                {
                    ["type"] = "boolean",
                    ["description"] =
                        "Set true when the customer specifically asks for medicines currently in stock."
                },

                ["requiresPrescription"] = new JsonObject
                {
                    ["type"] = "boolean",
                    ["description"] =
                        "Set true for prescription-required medicines or false for medicines that do not require a prescription."
                },

                ["productType"] = new JsonObject
                {
                    ["type"] = "string",
                    ["description"] =
                        "Product type such as Tablet, Syrup, Injection, Ointment, or Drops."
                },

                ["category"] = new JsonObject
                {
                    ["type"] = "string",
                    ["description"] =
                        "Medicine category name."
                },

                ["subCategory"] = new JsonObject
                {
                    ["type"] = "string",
                    ["description"] =
                        "Medicine subcategory name."
                },

                ["expiryWithinDays"] = new JsonObject
                {
                    ["type"] = "integer",
                    ["description"] =
                        "Return medicines whose stock expiry date is within this many days from today. " +
                        "Use only when the customer explicitly asks about expiry or medicines expiring soon."
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
            var search = GetString(arguments, "search");
            var maxPrice = GetDecimal(arguments, "maxPrice");
            var minPrice = GetDecimal(arguments, "minPrice");
            var inStock = GetBool(arguments, "inStock");
            var requiresPrescription = GetBool(arguments, "requiresPrescription");
            var productType = GetString(arguments, "productType");
            var category = GetString(arguments, "category");
            var subCategory = GetString(arguments, "subCategory");
            var expiryWithinDays = GetInt(arguments, "expiryWithinDays");

            var query = _db.Medicines
                .AsNoTracking()
                .Include(m => m.Category)
                .Include(m => m.SubCategory)
                .Include(m => m.ProductType)
                .Include(m => m.Stock)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();

                query = query.Where(m =>
                    EF.Functions.ILike(m.Name, $"%{term}%") ||
                    (m.GenericName != null &&
                     EF.Functions.ILike(m.GenericName, $"%{term}%")));
            }

            if (minPrice.HasValue)
            {
                query = query.Where(m => m.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(m => m.Price <= maxPrice.Value);
            }

            if (inStock == true)
            {
                query = query.Where(m =>
                    m.Stock != null &&
                    m.Stock.Quantity > 0);
            }

            if (requiresPrescription.HasValue)
            {
                query = query.Where(m =>
                    m.RequiresPrescription == requiresPrescription.Value);
            }

            if (!string.IsNullOrWhiteSpace(productType))
            {
                var type = productType.Trim();

                query = query.Where(m =>
                    EF.Functions.ILike(m.ProductType.Name, $"%{type}%"));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                var categoryTerm = category.Trim();

                query = query.Where(m =>
                    EF.Functions.ILike(m.Category.Name, $"%{categoryTerm}%"));
            }

            if (!string.IsNullOrWhiteSpace(subCategory))
            {
                var subCategoryTerm = subCategory.Trim();

                query = query.Where(m =>
                    m.SubCategory != null &&
                    EF.Functions.ILike(
                        m.SubCategory.Name,
                        $"%{subCategoryTerm}%"));
            }

            if (expiryWithinDays.HasValue)
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                var expiryLimit = today.AddDays(expiryWithinDays.Value);

                query = query.Where(m =>
                    m.Stock != null &&
                    m.Stock.ExpiryDate >= today &&
                    m.Stock.ExpiryDate <= expiryLimit);
            }

            var medicines = await query
                .OrderBy(m => m.Name)
                .Take(MaxResults)
                .Select(m => new
                {
                    m.Id,
                    m.Name,
                    m.GenericName,
                    m.Manufacturer,
                    m.Price,
                    m.Unit,
                    m.RequiresPrescription,
                    Category = m.Category.Name,
                    SubCategory = m.SubCategory != null
                        ? m.SubCategory.Name
                        : null,
                    ProductType = m.ProductType.Name,
                    Stock = m.Stock != null
                        ? m.Stock.Quantity
                        : 0,
                    ExpiryDate = m.Stock != null
                        ? m.Stock.ExpiryDate
                        : (DateOnly?)null
                })
                .ToListAsync(ct);

            return JsonSerializer.Serialize(new
            {
                success = true,
                count = medicines.Count,
                results = medicines
            });
        }

        private static string? GetString(
            JsonObject arguments,
            string name)
        {
            var node = arguments[name];

            if (node == null)
                return null;

            try
            {
                return node.GetValue<string>();
            }
            catch
            {
                return null;
            }
        }

        private static decimal? GetDecimal(
            JsonObject arguments,
            string name)
        {
            var node = arguments[name];

            if (node == null)
                return null;

            try
            {
                return node.GetValue<decimal>();
            }
            catch
            {
                try
                {
                    var text = node.GetValue<string>();

                    return decimal.TryParse(
                        text,
                        out var value)
                        ? value
                        : null;
                }
                catch
                {
                    return null;
                }
            }
        }

        private static int? GetInt(
            JsonObject arguments,
            string name)
        {
            var node = arguments[name];

            if (node == null)
                return null;

            try
            {
                return node.GetValue<int>();
            }
            catch
            {
                return null;
            }
        }

        private static bool? GetBool(
            JsonObject arguments,
            string name)
        {
            var node = arguments[name];

            if (node == null)
                return null;

            try
            {
                return node.GetValue<bool>();
            }
            catch
            {
                return null;
            }
        }
    }
}
namespace MediCart.Web.Data
{
    public class Medicine
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public int? SubCategoryId { get; set; }
        public int ProductTypeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? GenericName { get; set; }
        public string? Manufacturer { get; set; }
        public decimal Price { get; set; }
        public string? Unit { get; set; }
        public string? Description { get; set; }
        public bool RequiresPrescription { get; set; } = false;
        public string? SensitivityLevel { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Category Category { get; set; } = null!;
        public SubCategory? SubCategory { get; set; }
        public ProductType ProductType { get; set; } = null!;
        public Stock? Stock { get; set; }
        public ICollection<SideEffect> SideEffects { get; set; } = new List<SideEffect>();
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
namespace MediCart.Web.Data
{
    public class Stock
    {
        public int Id { get; set; }
        public int MedicineId { get; set; }
        public int Quantity { get; set; } = 0;
        public DateOnly ExpiryDate { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Medicine Medicine { get; set; } = null!;
        public ICollection<ExpiryAlert> ExpiryAlerts { get; set; } = new List<ExpiryAlert>();
    }
}
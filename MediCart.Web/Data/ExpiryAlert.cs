namespace MediCart.Web.Data
{
    public class ExpiryAlert
    {
        public int Id { get; set; }
        public int StockId { get; set; }
        public int MedicineId { get; set; }
        public string AlertLevel { get; set; } = string.Empty;
        public DateOnly AlertDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
        public bool IsResolved { get; set; } = false;

        public Stock Stock { get; set; } = null!;
        public Medicine Medicine { get; set; } = null!;
    }
}
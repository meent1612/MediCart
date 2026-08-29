namespace MediCart.Web.Data
{
    public class SideEffect
    {
        public int Id { get; set; }
        public int MedicineId { get; set; }
        public string Effect { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;

        public Medicine Medicine { get; set; } = null!;
    }
}
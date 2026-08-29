namespace MediCart.Web.Data
{
    public class City
    {
        public int Id { get; set; }
        public int DivisionId { get; set; }
        public string Name { get; set; } = string.Empty;

        public Division Division { get; set; } = null!;
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
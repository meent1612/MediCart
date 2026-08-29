namespace MediCart.Web.Data
{
    public class Division
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal DeliveryCharge { get; set; } = 120.00m;

        public ICollection<City> Cities { get; set; } = new List<City>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
namespace MediCart.Web.Data
{
    public class ProductType
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public ICollection<Medicine> Medicines { get; set; } = new List<Medicine>();
    }
}
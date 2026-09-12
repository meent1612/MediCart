namespace MediCart.Web.Models
{
    public class FlaggedItemViewModel
    {
        public string MedicineName { get; set; } = "";
        public string SensitivityLevel { get; set; } = ""; // "low" | "mid" | "high"
        public int Quantity { get; set; }
        public int Threshold { get; set; }
    }

    public class AdminFlaggedOrderRowViewModel
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public List<FlaggedItemViewModel> FlaggedItems { get; set; } = new();
    }

    public class AdminFlaggedOrdersListViewModel
    {
        public List<AdminFlaggedOrderRowViewModel> Orders { get; set; } = new();
        public int TotalCount { get; set; }

        // "All" | "High" | "Mid" | "Low"
        public string TierFilter { get; set; } = "All";
    }
}
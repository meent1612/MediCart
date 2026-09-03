using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MediCart.Web.Data;

namespace MediCart.Web.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        // Human-friendly order code shown to the user, e.g. "MC-10482"
        [Required]
        [MaxLength(20)]
        public string OrderCode { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        // "Pending review", "Shipped", "Delivered", "Rejected"
        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "Pending review";

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
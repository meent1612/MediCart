using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MediCart.Web.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Division> Divisions { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductType> ProductTypes { get; set; }
        public DbSet<Medicine> Medicines { get; set; }
        public DbSet<SideEffect> SideEffects { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<ExpiryAlert> ExpiryAlerts { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // City — unique constraint on (DivisionId, Name)
            builder.Entity<City>()
                .HasIndex(c => new { c.DivisionId, c.Name })
                .IsUnique();

            // Category — self-referencing
            builder.Entity<Category>()
                .HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Medicine — decimal column + check constraint
            builder.Entity<Medicine>()
                .Property(m => m.Price)
                .HasColumnType("numeric(10,2)");

            builder.Entity<Medicine>()
                .ToTable(t => t.HasCheckConstraint("CK_Medicine_SensitivityLevel",
                    "\"SensitivityLevel\" IN ('high','mid','low') OR \"SensitivityLevel\" IS NULL"));

            // SideEffect — check constraint on Severity
            builder.Entity<SideEffect>()
                .ToTable(t => t.HasCheckConstraint("CK_SideEffect_Severity",
                    "\"Severity\" IN ('mild','moderate','severe')"));

            // Stock — one-to-one with Medicine
            builder.Entity<Stock>()
                .HasOne(s => s.Medicine)
                .WithOne(m => m.Stock)
                .HasForeignKey<Stock>(s => s.MedicineId)
                .OnDelete(DeleteBehavior.Cascade);

            // CartItem — unique constraint on (UserId, MedicineId)
            builder.Entity<CartItem>()
                .HasIndex(c => new { c.UserId, c.MedicineId })
                .IsUnique();

            // Order — decimal columns + check constraint
            builder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasColumnType("numeric(10,2)");

            builder.Entity<Order>()
                .Property(o => o.DeliveryCharge)
                .HasColumnType("numeric(10,2)");

            builder.Entity<Order>()
                .ToTable(t => t.HasCheckConstraint("CK_Order_Status",
                    "\"Status\" IN ('Pending','Processing','Shipped','Delivered','Rejected')"));

            // Prescription — one-to-one with Order + check constraint
            builder.Entity<Prescription>()
                .HasOne(p => p.Order)
                .WithOne(o => o.Prescription)
                .HasForeignKey<Prescription>(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Prescription>()
                .ToTable(t => t.HasCheckConstraint("CK_Prescription_Status",
                    "\"Status\" IN ('pending','verified','rejected')"));

            // ExpiryAlert — check constraint on AlertLevel
            builder.Entity<ExpiryAlert>()
                .ToTable(t => t.HasCheckConstraint("CK_ExpiryAlert_AlertLevel",
                    "\"AlertLevel\" IN ('warning','critical')"));

            // AuditLog — AdminId is a string FK to AspNetUsers
            builder.Entity<AuditLog>()
                .HasOne(a => a.Admin)
                .WithMany()
                .HasForeignKey(a => a.AdminId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
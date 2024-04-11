using Microsoft.EntityFrameworkCore;

namespace Intextwo.Models
{
    public class LegoDbContext : DbContext
    {
        public LegoDbContext()
        {
        }

        public LegoDbContext(DbContextOptions<LegoDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuration for Order and lineItem relationship
            modelBuilder.Entity<Order>()
                .HasMany(o => o.LineItems)
                .WithOne(li => li.Order)
                .HasForeignKey(li => li.transaction_ID);

            // Primary key configuration for lineItem
            modelBuilder.Entity<lineItem>()
                .HasKey(li => new { li.transaction_ID, li.product_ID });

            // ... Other configurations ...
        }

        public virtual DbSet<Customer> customer { get; set; }
        public virtual DbSet<Product> products { get; set; }
        public virtual DbSet<Order> orders { get; set; }
        public virtual DbSet<lineItem> lineItems { get; set; }
        public virtual DbSet<ApplicationUser> AspNetUsers { get; set; }
        public virtual DbSet<UserCustomer> UserCustomer { get; set; }
        public virtual DbSet<Recommendation> recs { get; set; }
    }
}

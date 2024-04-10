using Microsoft.EntityFrameworkCore;

namespace brickit.Models
{
    public class LegoDbContext: DbContext
    {
        public LegoDbContext()
        {
        }


        public LegoDbContext(DbContextOptions<LegoDbContext> options) 
            : base(options)
        {
        }

        public virtual DbSet<Customer> customer { get; set; }
        public virtual DbSet<Product> products { get; set; }
        public virtual DbSet<Order> orders { get; set; }
        public virtual DbSet<lineItem> lineItems { get; set; }
        public virtual DbSet<User> users { get; set; }

    }
}

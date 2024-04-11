using Microsoft.EntityFrameworkCore;

namespace Intextwo.Models
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
        public virtual DbSet<ApplicationUser> AspNetUsers { get; set; }
        public virtual DbSet<Recommendation> recs { get; set; }


    }
}

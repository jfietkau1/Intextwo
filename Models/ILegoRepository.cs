namespace Intextwo.Models
{
    public interface ILegoRepository
    {
        IQueryable<Customer> Customers { get; }
        IQueryable<Order> Orders { get; }
        IQueryable<Product> Products { get; }
        IQueryable<lineItem> LineItems { get; }
        IQueryable<ApplicationUser> AspNetUsers { get; }
        IQueryable<UserCustomer> UserCustomers { get; }

        IEnumerable<string> GetUniqueCategories();
        IEnumerable<string> GetUniqueColors();


        IQueryable<Recommendation> recs { get; }

        void Add<T>(T entity) where T : class;

        void Remove<T>(T entity) where T : class;
        void Update<T>(T entity) where T : class;
        void SaveChanges();
        public Recommendation GetRecommendationForProduct(Product product);

    }
}

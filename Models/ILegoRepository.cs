namespace Intextwo.Models
{
    public interface ILegoRepository
    {
        IQueryable<Customer> Customers { get; }
        IQueryable<Order> Orders { get; }
        IQueryable<Product> Products { get; }
        IQueryable<lineItem> LineItems { get; }
        IQueryable<User> users { get; }


        void Add<T>(T entity) where T : class;

        void Remove<T>(T entity) where T : class;
        void Update<T>(T entity) where T : class;
        void SaveChanges();

    }
}

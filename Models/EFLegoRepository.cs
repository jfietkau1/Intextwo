﻿
namespace Intextwo.Models
{
    public class EFLegoRepository: ILegoRepository
    {
        private LegoDbContext _context;
        public EFLegoRepository(LegoDbContext temp)
        {
            _context = temp;
        }


        public IQueryable<Customer> Customers => _context.customer;
        public IQueryable<Order> Orders => _context.orders;
        public IQueryable<Product> Products => _context.products;
        public IQueryable<lineItem> LineItems => _context.lineItems;
        public IQueryable<ApplicationUser> AspNetUsers => _context.AspNetUsers;
        public IQueryable<UserCustomer> UserCustomers => _context.UserCustomer;
        public IQueryable<Recommendation> recs => _context.recs;



        public void Add<T>(T entity) where T : class
        {
            _context.Set<T>().Add(entity);
        }

        public void Remove<T>(T entity) where T : class
        {
            _context.Set<T>().Remove(entity);
        }

        public void Update<T>(T entity) where T : class
        {
            _context.Set<T>().Update(entity);
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        public IEnumerable<string> GetUniqueCategories()
        {
            return _context.products
                           .Select(p => p.category)
                           .Distinct()
                           .ToList();
        }
        public IEnumerable<string> GetUniqueColors()
        {
            return _context.products
                           .Select(p => p.primary_color)
                           .Distinct()
                           .OrderBy(color => color) // Optional, for sorting colors alphabetically
                           .ToList();
        }

        public Recommendation GetRecommendationForProduct(Product product)
        {
            // Retrieve recommendation for the product name
            var recommendation = _context.recs
                .Single(r => r.name == product.name);

            return recommendation;
        }

    }
}

namespace Intextwo.Models.ViewModels
{
    public class ProductListViewModel
    {
        public IQueryable<Product> Products { get; set;}

        public PaginationInfo PaginationInfo { get; set;} = new PaginationInfo();


    }
}

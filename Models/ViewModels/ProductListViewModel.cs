namespace Intextwo.Models.ViewModels
{
    public class ProductListViewModel
    {
        public IQueryable<Product> Products { get; set;}

        public PaginationInfo PaginationInfo { get; set;} = new PaginationInfo();

        public List<string>? Colors { get; set;}
        public List<string>? Categories { get; set;}
        public string? searchParam { get; set;}
    }
}

namespace Intextwo.Models.ViewModels
{
    public class OrderViewModel
    {
        public IQueryable<ProductOrderViewModel> OrdersAndCustomers { get; set; }
        public PaginationInfo PaginationInfo { get; set; } = new PaginationInfo();

    }
}

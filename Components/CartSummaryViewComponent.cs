using Microsoft.AspNetCore.Mvc;
using Intextwo.Models;

namespace Intextwo.Components
{
    public class CartSummaryViewComponent: ViewComponent
    {
        private Cart cart;
        public CartSummaryViewComponent(Cart cartService)
        {
            this.cart = cartService;
        }
        public IViewComponentResult Invoke()
        {
            return View(cart);
        }

    }
}

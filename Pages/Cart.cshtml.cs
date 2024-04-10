using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Intextwo.Models;
using Microsoft.Identity.Client;
using Microsoft.AspNetCore.Http;
using Intextwo.Infrastructure;

namespace Intextwo.Pages
{
    public class CartModel : PageModel
    {
        private readonly ILegoRepository _repo;
        public CartModel(ILegoRepository temp)
        {
            _repo = temp;
        }

        public Cart? Cart { get; set; }
        public string ReturnUrl { get; set; } = "/";

        public void OnGet(string returnUrl)
        {
            ReturnUrl = returnUrl ?? "/";
            Cart = HttpContext.Session.GetJson<Cart>("cart") ?? new Cart();
        }


        public IActionResult OnPost(int productid, string returnUrl)
        {
            Product prod = _repo.Products
                .FirstOrDefault(x => x.product_ID == productid);

            if(prod != null)
            {
                Cart = HttpContext.Session.GetJson<Cart>("cart") ?? new Cart();
                Cart.AddItem(prod, 1);
                HttpContext.Session.SetJson("cart", Cart);

            }
            return RedirectToPage(new {returnUrl = returnUrl});

        }


    }
}

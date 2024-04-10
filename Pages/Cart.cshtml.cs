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
        public Cart Cart { get; set; }

        public CartModel(ILegoRepository temp, Cart cartService)
        {
            _repo = temp;
            Cart = cartService;
        }

        public string ReturnUrl { get; set; } = "/";

        public void OnGet(string returnUrl)
        {
            ReturnUrl = returnUrl ?? "/";

        }


        public IActionResult OnPost(int productid, string returnUrl)
        {
            Product prod = _repo.Products
                .FirstOrDefault(x => x.product_ID == productid);

            if(prod != null)
            {
                Cart.AddItem(prod, 1);
            }
            return RedirectToPage(new {returnUrl = returnUrl});

        }
        public IActionResult OnPostRemove(int productid, string returnUrl)
        {
            //this fetches the cartline that has a matching productid and then passes that product into the removeline function
            //the remove line function expects a product object to remove
            Cart.RemoveLine(Cart.Lines.First(x => x.Product.product_ID == productid).Product);
            return RedirectToPage(new {returnUrl = returnUrl});
        }

    }
}

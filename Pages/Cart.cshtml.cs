using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Intextwo.Models;
using Microsoft.Identity.Client;
using Microsoft.AspNetCore.Http;

namespace Intextwo.Pages
{
    public class CartModel : PageModel
    {
        private readonly ILegoRepository _repo;
        public CartModel(ILegoRepository temp)
        {
            _repo = temp;
        }

        public Cart? cart { get; set; }

        public void OnGet()
        {

        }


        public void OnPost(int productid)
        {
            //this is producing an error when it tries to render its cshtml page because it says that it is
            //trying to render something that is null or is not set to an instance of the needed model.


            Product prod = _repo.Products
                .FirstOrDefault(x => x.product_ID == productid);

            Cart cart = new Cart();


            cart.AddItem(prod, 1);
        }


    }
}

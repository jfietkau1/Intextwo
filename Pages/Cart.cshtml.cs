using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Intextwo.Models;
using Microsoft.Identity.Client;
using Microsoft.AspNetCore.Http;
using Intextwo.Infrastructure;
using static Intextwo.Models.Cart;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace Intextwo.Pages
{
    public class CartModel : PageModel
    {
        private readonly ILegoRepository _repo;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IFraudPredictionService _fraudPredictionService;


        //these lists are used to randomly generate values for these fields when an order is created (because we don't have real data)
        private static readonly List<string> EntryModes = new List<string> { "Tap", "CVC", "PIN" };
        private static readonly List<string> TransactionTypes = new List<string> { "POS", "Online", "ATM" };
        private static readonly List<string> BankNames = new List<string> { "Metro", "Barclays", "Monzo", "Lloyds" };
        private static readonly List<string> CardTypes = new List<string> { "Visa", "MasterCard" };



        private static readonly Random Random = new Random();
        public Cart Cart { get; set; }

        public CartModel(ILegoRepository temp, Cart cartService, UserManager<IdentityUser> userManager, IFraudPredictionService fraudPredictionService)
        {
            _repo = temp;
            Cart = cartService;
            _userManager = userManager;
            _fraudPredictionService = fraudPredictionService;
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



        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> OnPostCheckout()
        {
            if (User.Identity.IsAuthenticated)
            {
                    IdentityUser user = await _userManager.GetUserAsync(User);

                if (_repo.UserCustomers.FirstOrDefault(x => x.Id == user.Id) == null)
                {
                    var userCustomer = new UserCustomer { Id = user.Id };
                    _repo.Add(userCustomer);
                    _repo.SaveChanges();
                } //verifies that the user has an entry in the UserCustomer table

                if (user != null)
                {


                    UserCustomer this_user = _repo.UserCustomers
                        .FirstOrDefault(x => x.Id == user.Id); // brings in the UserCustomer that was just verified


                    Customer customer = _repo.Customers.FirstOrDefault(x => x.customer_ID == this_user.customer_ID);//brings in the customer info                                                                                                                // checks to see if the customers table has an entry 
                    if (customer == null)
                    {
                        return RedirectToAction("CompleteRegistration", new { id = this_user.customer_ID });
                    }









                    //now we can get the items from the cart and enter them into the database
                    var (products, quantities) = ((SessionCart)Cart).GetCartProductsAndQuantities();
                    int max_trans_id = await _repo.Orders.MaxAsync(o => (int?)o.transaction_ID) ?? 0;



                    // Create a new order based on the cart content
                    var order = new Order();
                    // TODO: Populate the order details here
                    order.transaction_ID = max_trans_id + 1;
                    order.customer_ID = this_user.customer_ID;
                    order.date = DateTime.Now.Date;
                    order.day_of_week = DateTime.Now.DayOfWeek.ToString().Substring(0, 3);
                    order.time = DateTime.Now.Hour;
                    order.entry_mode = EntryModes[Random.Next(EntryModes.Count)];  //picks a random payment method bc we don't have actual data
                    order.amount = (int)(Math.Ceiling(Cart.CalculateTotal()));    //using the session cart calculatetotal function
                    order.type_of_transaction = TransactionTypes[Random.Next(TransactionTypes.Count)];
                    order.shipping_address = customer.country_of_residence; //sets the address to the customer info address
                    order.bank = BankNames[Random.Next(BankNames.Count)];
                    order.type_of_card = CardTypes[Random.Next(CardTypes.Count)];

                    //---------------------------------------------------------------------------------------------
                    bool isFraudulent = _fraudPredictionService.IsFraudulentOrder(order);
                    order.fraud = isFraudulent;

                    //---------------------------------------------------------------------------------------------
                    order.country_of_transaction = customer.country_of_residence;


                    _repo.Add(order);
                    _repo.SaveChanges();

                    // Save the order to get the OrderId if it's generated by the database

                    for (int i = 0; i < products.Count; i++)
                    {
                        int max_line_id = await _repo.LineItems.MaxAsync(o => (int?)o.transaction_ID) ?? 0;
                        lineItem lineItem = new lineItem // LineItem class should match your database schema
                        {
                            transaction_ID = order.transaction_ID,
                            product_ID = products[i].product_ID,
                            qty = quantities[i],
                            rating = order.transaction_ID // Make sure this is set correctly, perhaps after saving the order
                                                          // Set any additional fields required by your LineItem entity
                        };

                        // Add line item to repository

                        _repo.Add(lineItem);
                        customer.recentprod = lineItem.product_ID;
                        //_---------------------------------------------------------------------------------------------------------
                    }

                    // Save all changes to the database
                    _repo.SaveChanges();

                    // Clear the cart after successful checkout
                    ((SessionCart)Cart).Clear();





                    if(order.fraud == true)
                    {
                        return RedirectToAction("OrderPending");
                    }
                    else
                    {
                        return RedirectToAction("OrderConfirmation");


                    }

                }
                else
                {
                    return RedirectToPage("/Account/Login", new { Area = "Identity" });
                }
            }
            else
            {
                return RedirectToPage("/Account/Login", new { Area = "Identity" });
            }
        }

    }
}

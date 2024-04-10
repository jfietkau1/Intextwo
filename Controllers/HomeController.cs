using brickit.Models;
using Intextwo.Models;
using Intextwo.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Core.Types;
using System.Diagnostics;
using System.Linq;


namespace Intextwo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILegoRepository _repo;

        public HomeController( ILegoRepository temp)
        {
            _repo = temp;
        }

        public IActionResult Index()
        {

            var viewModelList = _repo.Orders
                .Join(
                    _repo.Customers,
                    order => order.customer_ID,
                    customer => customer.customer_ID,
                    (order, customer) => new OrderViewModel
                    {
                        // Order properties
                        transaction_ID = order.transaction_ID,
                        date = order.date,
                        day_of_week = order.day_of_week,
                        time = order.time,
                        entry_mode = order.entry_mode,
                        amount = order.amount,
                        type_of_transaction = order.type_of_transaction,
                        shipping_address = order.shipping_address,
                        bank = order.bank,
                        type_of_card = order.type_of_card,
                        fraud = order.fraud,

                        // Customer properties
                        customer_ID = customer.customer_ID,
                        first_name = customer.first_name,
                        last_name = customer.last_name,
                        birth_date = customer.birth_date,
                        country_of_residence = customer.country_of_residence,
                        gender = customer.gender,
                        age = customer.age
                    }
                ).Take(5).ToList();

            return View(viewModelList);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

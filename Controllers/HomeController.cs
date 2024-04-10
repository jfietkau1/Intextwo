using Intextwo.Models;
using Intextwo.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
                ).Take(1).ToList();

            return View(viewModelList);
        }

        [HttpGet]
        public IActionResult CustProductList(int pageNum, string? searchParam)
        {
            if (pageNum == 0) { pageNum = 1; }
            var pageSize = 6;// this is the page size

            var productQuery = _repo.Products
            .Where(x => searchParam == null || EF.Functions.Like(x.name, $"%{searchParam}%")); //executes part of the query


            var viewModel = new ProductListViewModel()
            {

                // this statement pulls in the product data, sets the number for this page, and will also query for a specific
                //string if you pass one in with a search bar. 
                Products = productQuery
                .OrderBy(x => x.name)
                .Skip((pageNum - 1) * pageSize)
                .Take(pageSize),
                PaginationInfo = new PaginationInfo() 
                {
                CurrentPage = pageNum,
                ItemsPerPage = pageSize,
                TotalItems = productQuery.Count()

                }
            };

            return View(viewModel);
        }

        public IActionResult AboutUs()
        {
            return View();
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

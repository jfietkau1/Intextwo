using Intextwo.Models;
using Intextwo.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Core.Types;
using System.Diagnostics;
using System.Linq;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Elfie.Serialization;
using Microsoft.AspNetCore.Authorization;


namespace Intextwo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILegoRepository _repo;
        private readonly InferenceSession _session; // ONNX Runtime InferenceSession

        public HomeController( ILegoRepository temp)
        {
            _repo = temp;
            _session = new InferenceSession("wwwroot/fraud_pred.onnx");

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

        [Authorize (Roles = "Admin")]
        public IActionResult AdminMenu()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult AdminAddProduct()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult AdminAddProduct(Product product)
        {
            _repo.Add(product);
            _repo.SaveChanges();
            return RedirectToAction("AdminProductList");
        }

        [Authorize(Roles = "Admin")]
        public IActionResult AdminProductList()
        {
            var products = _repo.Products.ToList();

            return View(products);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult EditProd(int id)
        {
            var recordToEdit = _repo.Products
                .Single(x => x.product_ID == id);
            return View("AdminAddProduct", recordToEdit);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult EditProd(Product prod)
        {
            _repo.Update(prod);
            _repo.SaveChanges();

            return RedirectToAction("AdminProductList");
        }

        [HttpGet]
        public IActionResult DeleteProd(int id)
        {
            var recordToDelete = _repo.Products
                .Single(x => x.product_ID == id);
            return View(recordToDelete);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProd(Product prod)
        {
            //var product = await _repo.Products.FirstOrDefaultAsync(p => p.product_ID == id);
            var product = prod;

            _repo.Remove(product); // Correctly removes the product from the repository
            _repo.SaveChanges();

            return RedirectToAction("AdminProductList"); // Assuming you have a ProductList action
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
        public IActionResult Sorry()
        {
            return View();
        }


      
        [HttpGet]
        public IActionResult ViewProduct(int id)
        {
            var recordToSee = _repo.Products
                .Single(x => x.product_ID == id);
            return View(recordToSee);
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

        [HttpGet]
        public IActionResult SeeProd(int id)
        {
            var recordToSee = _repo.Products
                .Single(x => x.product_ID == id);
            return View("IndividualProduct", recordToSee);
        }

        
        //post method for predicting fraud using the onnx model 
        [HttpPost]
        public IActionResult Predict(int day_of_week_Mon, int day_of_week_Sat, int day_of_week_Sun, int day_of_week_Thu, int day_of_week_Tue, int day_of_week_Wed, int time_1, int time_2, int time_3, int time_4, int time_5, int time_6, int time_7, int time_8, int time_9, int time_10, int time_11, int time_12, int time_13, int time_14, int time_15, int time_16, int time_17, int time_18, int time_19, int time_20, int time_21, int time_22, int time_23, int time_24, int entry_mode_PIN, int entry_mode_Tap, int type_of_transaction_Online, int type_of_transaction_POS, int country_of_transaction_India, int country_of_transaction_Russia, int country_of_transaction_USA, int country_of_transaction_United_Kingdom, int shipping_address_India, int shipping_address_Russia, int shipping_address_USA, int shipping_address_United_Kingdom, int bank_HSBC, int bank_Halifax, int bank_Lloyds, int bank_Metro, int bank_Monzo, int bank_RBS, int type_of_card_Visa)
        {

            var class_type_dict = new Dictionary<int, string>
    {
                { 0, "not fraud" },
                { 1, "fraud" }
    };

            try
            {
                Random random = new Random();
                var input = new List<float> { day_of_week_Mon, day_of_week_Sat, day_of_week_Sun, day_of_week_Thu, day_of_week_Tue, day_of_week_Wed, time_1, time_2, time_3, time_4, time_5, time_6, time_7, time_8, time_9, time_10, time_11, time_12, time_13, time_14, time_15, time_16, time_17, time_18, time_19, time_20, time_21, time_22, time_23, time_24, entry_mode_PIN, entry_mode_Tap, type_of_transaction_Online, type_of_transaction_POS, country_of_transaction_India, country_of_transaction_Russia, country_of_transaction_USA, country_of_transaction_United_Kingdom, shipping_address_India, shipping_address_Russia, shipping_address_USA, shipping_address_United_Kingdom, bank_HSBC, bank_Halifax, bank_Lloyds, bank_Metro, bank_Monzo, bank_RBS, type_of_card_Visa };



                var inputTensor = new DenseTensor<float>(input.ToArray(), new[] { 1, input.Count });
                var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("float_input", inputTensor)
        };

                using (var results = _session.Run(inputs)) // makes the prediction with the inputs from the form (i.e. class_type 1-7)
                {
                    var prediction = results.FirstOrDefault(item => item.Name == "output_label")?.AsTensor<long>().ToArray();
                    if (prediction != null && prediction.Length > 0)
                    {
                        // Use the prediction to get the animal type from the dictionary
                        var fraud = class_type_dict.GetValueOrDefault((int)prediction[0], "Unknown");
                        //ViewBag.Prediction = fraud;
                        if (fraud == "fraud")
                        {
                            ViewBag.Prediction = "Order processing"; // Set view message if fraud detected
                            return View("Index");
                        }
                        else
                        {
                            ViewBag.Prediction = "Order fulfilled"; // Set view message if no fraud detected
                            return View("Index");
                        }
                    }

                    else
                    {
                        ViewBag.Prediction = "Error: Unable to make a prediction.";
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Prediction = $"Error: {ex.Message}";
            }

            return View("Index");
        }
    }
}

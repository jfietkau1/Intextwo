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
using System.Drawing.Printing;
using Microsoft.Identity.Client;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;


namespace Intextwo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILegoRepository _repo;
        private readonly InferenceSession _session; // ONNX Runtime InferenceSession
        private readonly UserManager<IdentityUser> _userManager;
        public readonly RoleManager<IdentityRole> _roleManager;

        public HomeController(ILegoRepository temp, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _repo = temp;
            _session = new InferenceSession("wwwroot/fraud_pred.onnx");
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            List<Product> products = new List<Product>();

            IdentityUser user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                // Assuming you have a Customers table and each customer has a 'recentprod' property
                var customerID = await _repo.UserCustomers
                                            .Where(uc => uc.Id == user.Id)
                                            .Select(uc => uc.customer_ID)
                                            .FirstOrDefaultAsync();

                if (customerID != default)
                {
                    // Retrieve the recent product ID for this customer
                    var recentProductId = await _repo.Customers
                                                     .Where(c => c.customer_ID == customerID)
                                                     .Select(c => c.recentprod)
                                                     .FirstOrDefaultAsync();

                    // Assuming the recent product ID is not 0 or a default value that indicates no recent product
                    if (recentProductId > 0)
                    {
                        // Retrieve the product details for the recent product ID
                        var recentProduct = await _repo.Products
                                                       .FirstOrDefaultAsync(p => p.product_ID == recentProductId);

                        var recommender = await _repo.recs
                                                        .FirstOrDefaultAsync(x => x.name == recentProduct.name);
                        List<string> names = new List<string>();

                        // Add recommendations to the list
                        names.Add(recommender.Recommendation1);
                        names.Add(recommender.Recommendation2);
                        names.Add(recommender.Recommendation3);
                        names.Add(recommender.Recommendation4);
                        names.Add(recommender.Recommendation5);

                        foreach(var name in names)
                        {
                            products.Add(_repo.Products.FirstOrDefault(x => x.name == name));
                        }

                        // If the product exists, pass it to the view, else pass an empty list
                        return View(products);
                    }
                }
            }

            List<int> numbers = new List<int> { 24, 20, 22, 21, 13 };
            foreach(var i in numbers)
            {
                products.Add(_repo.Products.FirstOrDefault(x => x.product_ID == i));
            }

            // If no user, customer, or order is found, return an empty list to the view
            return View(products);
        }





        [Authorize(Roles = "Admin")]
        public IActionResult AdminMenu()
        {
            return View();
        }
        [Authorize(Roles = "Admin")]
        public IActionResult AdminUserEdit()
        {
            var users = _repo.AspNetUsers;
            return View("AdminUserEdit", users);
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult AdminAddUser()
        {
            var newUser = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(), // Generate a new GUID for Id
                UserName = "", // Set the default username
                NormalizedUserName = "", // Set the default normalized username
                NormalizedEmail = "", // Set the default normalized email
                EmailConfirmed = false, // Email is not confirmed by default
                PasswordHash = "", // Set the default password hash
                SecurityStamp = "", // Set the default security stamp
                ConcurrencyStamp = Guid.NewGuid().ToString(), // Generate a new GUID for concurrency stamp
                PhoneNumber = "", // Set the default phone number
                PhoneNumberConfirmed = false, // Phone number is not confirmed by default
                TwoFactorEnabled = false, // Two-factor authentication is not enabled by default
                LockoutEnd = null, // No lockout end by default
                LockoutEnabled = false, // Lockout is not enabled by default
                AccessFailedCount = 0 // No access failures by default
            };
            return View(newUser);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult AdminAddUser(ApplicationUser user)
        {
            _repo.Add(user);
            _repo.SaveChanges();
            return RedirectToAction("AdminUserEdit");
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteUser(string id)
        {
            ApplicationUser recordToDelete = _repo.AspNetUsers
                .Single(x => x.Id == id);
            _repo.Remove(recordToDelete);
            _repo.SaveChanges();
            return RedirectToAction("AdminUserEdit");
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult AdminUpdateUser(string id)
        {
            var recordToEdit = _repo.AspNetUsers
                .Single(x => x.Id == id);
            return View("AdminUpdateUser", recordToEdit);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult AdminUpdateUser(ApplicationUser user)
        {
            _repo.Update(user);
            _repo.SaveChanges();

            return RedirectToAction("AdminUserEdit");
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
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteProd(int id)
        {
            var recordToDelete = _repo.Products
                .Single(x => x.product_ID == id);
            return View(recordToDelete);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProd(Product prod)
        {
            //var product = await _repo.Products.FirstOrDefaultAsync(p => p.product_ID == id);
            var product = prod;

            _repo.Remove(product); // Correctly removes the product from the repository
            _repo.SaveChanges();

            return RedirectToAction("AdminProductList"); // Assuming you have a ProductList action
        }

        [HttpGet]
        public IActionResult CustProductList(int pageNum, string? searchParam, int? pageSizes, string? cat, string? color)
        {
            List<string> colors = new List<string>();
            List<string> categories = new List<String>();
            if (pageNum == 0)
            {
                pageNum = 1;
            }
            int pageSize = pageSizes ?? 6;
            if (pageSizes != null)
            //if (pageNum == 0) { pageNum = 1; }
            //int pageSize = 6;
            //if(pageSizes != null)
            {
                pageSize = pageSizes.Value;
            }

            var cats = _repo.GetUniqueCategories(); //fetch unique categories
            var colorssss = _repo.GetUniqueColors(); // Fetch unique colors
                
            foreach (var i in cats)
            {
                categories.Add(i);
            }
            foreach(var i in colorssss)
            {
                colors.Add(i);
            }


            var productQuery = _repo.Products
            .Where(x => (searchParam == null || EF.Functions.Like(x.name, $"%{searchParam}%"))
                    && (cat == null || x.category == cat) // Checks if category filter is provided
                    && (color == null || x.primary_color == color)); // Checks if color filter is provided


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

                },
                Categories = categories,
                Colors = colors,
                searchParam = searchParam
                //add categories and colors here
            };

            return View(viewModel);
        }
        public IActionResult Sorry()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult AdminViewOrders(int pageNum)
        {
            if (pageNum == 0)
            {
                pageNum = 1;
            }
            var pageSize = 50;
            var query = _repo.Orders // This method needs to exist in your repository
                        .Join(_repo.Customers, // This method also needs to be defined
                            order => order.customer_ID, // Assumes Order has a CustomerId
                            customer => customer.customer_ID, // Assumes Customer has an Id
                            (order, customer) => new ProductOrderViewModel
                            {
                                // Map properties from Order
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

                                // Map properties from Customer
                                customer_ID = customer.customer_ID,
                                first_name = customer.first_name,
                                last_name = customer.last_name,
                                birth_date = customer.birth_date,
                                country_of_residence = customer.country_of_residence,
                                gender = customer.gender,
                                age = customer.age // Ensure your model supports this directly or adjust accordingly
                            })
                        .OrderByDescending(o => o.date)
                        .AsQueryable();


            var skipAmount = (pageNum - 1) * pageSize;
            var productOrders = query.Skip(skipAmount).Take(pageSize).ToList();
            var totalItems = query.Count();

            var viewModel = new OrderViewModel
            {
                OrdersAndCustomers = productOrders.AsQueryable(),
                PaginationInfo = new PaginationInfo
                {
                    CurrentPage = pageNum,
                    ItemsPerPage = pageSize,
                    TotalItems = totalItems
                }
            };

            return View(viewModel);
        }

        public IActionResult OrderConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult CompleteRegistration(int id)
        {
            ViewBag.customer_ID = id;
            return View();
        }
        [HttpPost]
        public IActionResult CompleteRegistration(Customer customer)
        {

            if (!ModelState.IsValid)
            {
                // The model is invalid, return the same form to show validation errors
                return View(customer);
            }
            //logic to add items to the database when the form is submitted.
            _repo.Add(customer);
            _repo.SaveChanges();
            return RedirectToPage("/Cart");
        }




        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            // Get all roles
            var roles = _roleManager.Roles.Select(r => r.Name).ToList();
            // Get the roles the user is currently in
            var userRoles = await _userManager.GetRolesAsync(user);
            // Pass user, roles, and user�s current roles to the view
            var model = new EditUserViewModel
            {
                User = user,
                Roles = roles,
                SelectedRoles = userRoles.ToList()
            };
            return View(model);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.User.Id);
            if (user == null)
            {
                return NotFound();
            }
            // Assume other properties are updated if needed
            user.Email = model.User.Email;

            var userRoles = await _userManager.GetRolesAsync(user);
            var removeResult = await _userManager.RemoveFromRolesAsync(user, userRoles);
            if (!removeResult.Succeeded)
            {
                // Handle error, possibly by adding to ModelState and returning View(model)
                return View(model);
            }
            var addResult = await _userManager.AddToRolesAsync(user, model.SelectedRoles);
            if (!addResult.Succeeded)
            {
                // Handle error, possibly by adding to ModelState and returning View(model)
                return View(model);
            }
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                // Handle error, possibly by adding to ModelState and returning View(model)
                return View(model);
            }

            return RedirectToAction("AdminUserEdit"); // Confirm this is the correct route
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
            List<Product> products = new List<Product>();
            var recordToSee = _repo.Products
                .Single(x => x.product_ID == id);

            var recommender = _repo.recs
                                 .FirstOrDefault(x => x.name == recordToSee.name);
            List<string> names = new List<string>();

            names.Add(recommender.Recommendation1);
            names.Add(recommender.Recommendation2);
            names.Add(recommender.Recommendation3);
            names.Add(recommender.Recommendation4);
            names.Add(recommender.Recommendation5);

            foreach (var name in names)
            {
                products.Add(_repo.Products.FirstOrDefault(x => x.name == name));
            }
            var viewModel = new ProductRecommendationViewModel

            {
                Product = recordToSee,
                Recommendations = products
            };

            return View("IndividualProduct", viewModel);
        }



        public IActionResult OrderPending()
        {
            return View();
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
                
                var input = new List<float>
                {
                    day_of_week_Mon, day_of_week_Sat, day_of_week_Sun, day_of_week_Thu, day_of_week_Tue, day_of_week_Wed,
                    time_1, time_2, time_3, time_4, time_5, time_6, time_7, time_8, time_9, time_10, time_11, time_12,
                    time_13, time_14, time_15, time_16, time_17, time_18, time_19, time_20, time_21, time_22, time_23,
                    time_24, entry_mode_PIN, entry_mode_Tap, type_of_transaction_Online, type_of_transaction_POS,
                    country_of_transaction_India, country_of_transaction_Russia, country_of_transaction_USA,
                    country_of_transaction_United_Kingdom, shipping_address_India, shipping_address_Russia,
                    shipping_address_USA, shipping_address_United_Kingdom, bank_HSBC, bank_Halifax, bank_Lloyds,
                    bank_Metro, bank_Monzo, bank_RBS, type_of_card_Visa
                };

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
                            return View("FraudCheck");
                        }
                        else
                        {
                            ViewBag.Prediction = "Order fulfilled"; // Set view message if no fraud detected
                            return View("FraudCheck");
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


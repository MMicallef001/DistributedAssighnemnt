using Common.Models;
using ECommerceApp.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Common;
using Newtonsoft.Json;
using static Google.Rpc.Context.AttributeContext.Types;
using System.Collections.Generic;
using System.Web;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Security.Policy;
using ECommerceApp.DataAccess;
using static Google.Cloud.Firestore.V1.StructuredQuery.Types;

namespace ECommerceApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private bool loggedIn =false;
        PubSubRepositary _pubSub;
        ShippingPubSubRepo _shippingPrubSubRepo;


        public HomeController(ILogger<HomeController> logger, PubSubRepositary pubSub, ShippingPubSubRepo shippingPrubSubRepo)
        {
            _logger = logger;
            _pubSub = pubSub;
            _shippingPrubSubRepo = shippingPrubSubRepo; 
        }

        public IActionResult Index()
        {
            TempData["loggedIn"] = loggedIn;
            return View();
        }

        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> RegisterUser()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> RegisterUser(User user)
        {
            using (var client = new HttpClient())
            {
                user.Id = Guid.NewGuid().ToString();
                client.BaseAddress = new Uri("https://customerapi-pqkchsrqxa-uc.a.run.app/api/UserMicroservice/");
                //client.BaseAddress = new Uri("https://localhost:7097/api/UserMicroservice/");




                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.PostAsJsonAsync("RegisterUser", user);

                if (response.IsSuccessStatusCode)
                {
                    var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Surname, user.SurName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.NameIdentifier, user.Id)
                    
                };

                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties();

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    return RedirectToAction("Categories");
                }
            }

            return RedirectToAction("Index");


        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
            {
                using (var client = new HttpClient())
                {
                    var credentials = new { email = email, password = password };

                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage response = await client.PostAsJsonAsync("https://customerapi-pqkchsrqxa-uc.a.run.app/api/UserMicroservice/LoginUser/", credentials);
                    //HttpResponseMessage response = await client.PostAsJsonAsync("https://localhost:7097/api/UserMicroservice/LoginUser/", credentials);


                    if (response.IsSuccessStatusCode)
                {
                    string uid = await response.Content.ReadAsAsync<string>();

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, email),
                        new Claim(ClaimTypes.NameIdentifier, uid)
                    };

                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties();

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    return RedirectToAction("Categories");
                    }
                }
            }
            return View();
        }



        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserDetails()
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync("https://customerapi-pqkchsrqxa-uc.a.run.app/api/UserMicroservice/" + userId);



                if (response.IsSuccessStatusCode)
                {

                    User userDetails = await response.Content.ReadAsAsync<User>();

                    //orderDetails.ProductUrl = HttpUtility.UrlDecode(orderDetails.ProductUrl);

                    return View("GetUserDetails", userDetails);
                }

            }
            return View("index");
        }


        [HttpGet]
        public async Task<IActionResult> Login()
        {
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index");
        }


        [HttpGet][Authorize]
        public IActionResult Categories()
        {
            List<ProductCategory> categories = new List<ProductCategory>
            {
                new ProductCategory { Name = "Laptop" },
                new ProductCategory { Name = "Headphones" },
                new ProductCategory { Name = "keyboard" },
                new ProductCategory { Name = "webcam" },
                new ProductCategory { Name = "mouse" },
            };

            return View(categories);
        }

        [HttpGet]
        [Authorize]
        public IActionResult Products(List<Product> productList)
        {
            return View(productList);
        }


        [HttpPost]
        public async Task<IActionResult> CategoryClickedAsync(string categoryName)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://productcatalougemicroservice-pqkchsrqxa-uc.a.run.app/api/ProductsMicroservice/");
                
                //client.BaseAddress = new Uri("https://localhost:7074/api/ProductsMicroservice/");

            

                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.PostAsJsonAsync("LoadProducts/" + categoryName, string.Empty);

                if (response.IsSuccessStatusCode)
                {

                    List<Product> products = await response.Content.ReadAsAsync<List<Product>>();
                    return View("Products", products);
                }
            }

            return RedirectToAction("Categories");
        }

        [HttpPost]
        public async Task<IActionResult> ViewDetails(string asin)
        {
            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync("https://productcatalougemicroservice-pqkchsrqxa-uc.a.run.app/api/ProductsMicroservice/" + asin);
                //HttpResponseMessage response = await client.GetAsync("https://localhost:7074/api/ProductsMicroservice/" + asin);


                if (response.IsSuccessStatusCode)
                {

                    ProductDetail productDetail = await response.Content.ReadAsAsync<ProductDetail>();

                    return View("ViewDetails", productDetail);
                }
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult ViewDetails(ProductDetail ProductDetails)
        {
            return View(ProductDetails);
        }


        [HttpPost]
        public async Task<IActionResult> Order(string asin)
        {

            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


                HttpResponseMessage response = await client.GetAsync("https://productcatalougemicroservice-pqkchsrqxa-uc.a.run.app/api/ProductsMicroservice/" + asin);
                //HttpResponseMessage response = await client.GetAsync("https://localhost:7074/api/ProductsMicroservice/" + asin);



                if (response.IsSuccessStatusCode)
                {


                    ProductDetail productDetail = await response.Content.ReadAsAsync<ProductDetail>();

                    string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    

                    string OrderId = Guid.NewGuid().ToString();


                    return RedirectToAction("Payment", new { price = productDetail.Pricing, orderId = OrderId, ProductAsin = asin });

                        
                }
            }
            //send to pay
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Payment(double price, string orderId,string ProductAsin)
        {
            PaymentViewModel pvm = new PaymentViewModel
            {
                Price = price,
                OrderId = orderId,
                Asin = ProductAsin
            };

            return View(pvm);
        }


        [HttpPost]
        public async Task<IActionResult> Payment(Payment payment, string OrderId, double Price, string asin)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;


            transferingModel tm = new transferingModel();
            tm.Asin = asin;
            tm.CardNumber = payment.CardNumber;
            tm.Addess = payment.Address;
            tm.UserId = userId;
            //pub sub 

            _pubSub.PushMessage(tm);

            return View("index");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ViewAllOrthers()
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            

                HttpResponseMessage Response = await client.GetAsync("https://ordersmicroservice-pqkchsrqxa-uc.a.run.app/api/OrdersMicroservice/user/" + userId);
                //HttpResponseMessage Response = await client.GetAsync("https://localhost:7202/api/OrdersMicroservice/user/" + userId);

                if (Response.IsSuccessStatusCode)
                {
                    List<Common.Models.Order> orders = await Response.Content.ReadAsAsync<List<Common.Models.Order>>();
                    return View("ViewAllOrthers", orders);
                }
                
            }
            return View("index");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetOrderDetails(string OrderId)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            using (var orderClient = new HttpClient())
            {
                orderClient.DefaultRequestHeaders.Clear();
                orderClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage OrderedResponse = await orderClient.GetAsync("https://ordersmicroservice-pqkchsrqxa-uc.a.run.app/api/OrdersMicroservice/GetOrderDetails/" + OrderId);
                //tpResponseMessage OrderedResponse = await orderClient.GetAsync("https://localhost:7202/api/OrdersMicroservice/GetOrderDetails/" + OrderId);


                if (OrderedResponse.IsSuccessStatusCode)
                {

                    Common.Models.Order orderDetails = await OrderedResponse.Content.ReadAsAsync<Common.Models.Order>();

                    //orderDetails.ProductUrl = HttpUtility.UrlDecode(orderDetails.ProductUrl);

                    return View("GetOrderDetails", orderDetails);
                }

            }
            return View("index");
        }



        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetPaymentsDetails(string OrderId)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            using (var orderClient = new HttpClient())
            {
                orderClient.DefaultRequestHeaders.Clear();
                orderClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage OrderedResponse = await orderClient.GetAsync("https://paymentmicroservice-pqkchsrqxa-uc.a.run.app/api/PaymentsMicroservice/GetPaymentDetails/" + OrderId);
                //HttpResponseMessage OrderedResponse = await orderClient.GetAsync("https://localhost:7153/api/PaymentsMicroservice/GetPaymentDetails/" + OrderId);


                if (OrderedResponse.IsSuccessStatusCode)
                {

                    Payment paymentDetails = await OrderedResponse.Content.ReadAsAsync<Payment>();

                   

                    return View("GetPaymentsDetails", paymentDetails);
                }

            }
            return View("index");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetShipments()
        {


            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync("https://shippingmicroservice-pqkchsrqxa-uc.a.run.app/api/ShippingMicroservice/allShipments/");
                //HttpResponseMessage response = await client.GetAsync("https://localhost:7293/api/ShippingMicroservice/allShipments/");


            

                if (response.IsSuccessStatusCode)
                {

                    List<Shipment> shipments = await response.Content.ReadAsAsync<List<Shipment>>();



                    return View("GetShipments", shipments);
                }

            }
            return View("index");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserNotifications()
        {

            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync("https://notificationmicroservice-pqkchsrqxa-uc.a.run.app/api/NotificationMicroservice/user/" + userId);
                //HttpResponseMessage response = await client.GetAsync("https://localhost:7293/api/ShippingMicroservice/allShipments/");




                if (response.IsSuccessStatusCode)
                {

                    List<Notification> notifications = await response.Content.ReadAsAsync<List<Notification>>();



                    return View("GetUserNotifications", notifications);
                }

            }
            return View("index");
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateShipment(Shipment s)
        {

            //string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


                HttpResponseMessage response = await client.GetAsync("https://ordersmicroservice-pqkchsrqxa-uc.a.run.app/api/OrdersMicroservice/GetOrderDetails/" + s.OrderId);
                //HttpResponseMessage UpdatedOrderedResponse = await client.PostAsJsonAsync("https://localhost:7293/api/ShippingMicroservice/update/", s);

                if (response.IsSuccessStatusCode)
                {
                    Common.Models.Order orderDetails = await response.Content.ReadAsAsync<Common.Models.Order>();
                    _shippingPrubSubRepo.PushMessage(s, orderDetails.UserId);

                    return View("index");
                }
            }

            return View("Index");

        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> UpdateShipment(string orderId)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


                HttpResponseMessage response = await client.GetAsync("https://shippingmicroservice-pqkchsrqxa-uc.a.run.app/api/ShippingMicroservice/GetShipmentDetails/" + orderId);
                //HttpResponseMessage response = await client.GetAsync("https://localhost:7293/api/ShippingMicroservice/GetShipmentDetails/" + orderId);

                if (response.IsSuccessStatusCode)
                {
                    Shipment shipment = await response.Content.ReadAsAsync<Shipment>();
                    return View("UpdateShipment", shipment);
                }
            }
            return View("index");


        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetShipmentDetails(string orderId)
        {

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync("https://shippingmicroservice-pqkchsrqxa-uc.a.run.app/api/ShippingMicroservice/GetShipmentDetails/" + orderId);


                if (response.IsSuccessStatusCode)
                {

                    Shipment shipmentDetails = await response.Content.ReadAsAsync<Shipment>();

                    //orderDetails.ProductUrl = HttpUtility.UrlDecode(orderDetails.ProductUrl);

                    return View("GetShipmentDetails", shipmentDetails);
                }

            }
            return View("index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
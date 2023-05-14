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

namespace ECommerceApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private bool loggedIn =false;


        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
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
                client.BaseAddress = new Uri("https://localhost:7097/api/UserMicroservice/"); 
                  
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
                    // Add other claims as needed.
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
                else
                {
                    // Do something in case of error
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
                    HttpResponseMessage response = await client.PostAsJsonAsync("https://localhost:7097/api/UserMicroservice/LoginUser/", credentials);


                    if (response.IsSuccessStatusCode)
                {
                    string uid = await response.Content.ReadAsAsync<string>();
                    // Create a session for the user.
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, email),
                        new Claim(ClaimTypes.NameIdentifier, uid)

                        // Add other claims as needed.
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
                    else
                    {
                        // Log the error or do something
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View();
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
                client.BaseAddress = new Uri("https://localhost:7074/api/ProductsMicroservice/");

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
        public async Task<IActionResult> ViewDetails(string url)
        {
            using (var client = new HttpClient())
            {
                //client.BaseAddress = new Uri("https://localhost:7074/api/ProductsMicroservice/");

                string encodedUrl = HttpUtility.UrlEncode(url);

                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync("https://localhost:7074/api/ProductsMicroservice/" + encodedUrl);

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
        public async Task<IActionResult> Order(string url)
        {
            using (var client = new HttpClient())
            {
                url = HttpUtility.UrlEncode(url);


                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync("https://localhost:7074/api/ProductsMicroservice/" + url);

                if (response.IsSuccessStatusCode)
                {


                    ProductDetail productDetail = await response.Content.ReadAsAsync<ProductDetail>();

                    string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                    Order o = new Order();

                    
                    o.OrderId = Guid.NewGuid().ToString();
                    o.ProductId = productDetail.Id;
                    o.UserId = userId;
                    o.ProductUrl = url;
                    o.ProductName = productDetail.ProductName;
                    o.image = productDetail.Image;
                    o.PaymentId = "";

                    
                   
                    if(!(productDetail.ShippingPrice.ToLower().Equals("free")))
                    {
                        string stringPrice = Regex.Replace(productDetail.ShippingPrice, "\\$", "");
                        double shipping = double.Parse(stringPrice);

                        stringPrice = Regex.Replace(productDetail.Pricing, "\\$", "");
                        double price = double.Parse(stringPrice);

                        double totalPrice = shipping + price;
                        o.Price = totalPrice;


                    }
                    else
                    {
                        string stringPrice = Regex.Replace(productDetail.Pricing, "\\$", "");
                        o.Price = double.Parse(stringPrice);
                    }

                    o.Status = "Order Is Waiting Payment";
                    o.Paid = false;

                    using (var orderClient = new HttpClient())
                    {
                        orderClient.BaseAddress = new Uri("https://localhost:7202/api/OrdersMicroservice/");

                        orderClient.DefaultRequestHeaders.Clear();
                        orderClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        HttpResponseMessage OrderedResponse = await orderClient.PostAsJsonAsync("AddOrder", o);

                        if (OrderedResponse.IsSuccessStatusCode)
                        {
                            return RedirectToAction("Payment", new { price = o.Price, orderId = o.OrderId });
                        }
                    }
                        
                }
            }
            //send to pay
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Payment(double price, string orderId)
        {
            PaymentViewModel pvm = new PaymentViewModel
            {
                Price = price,
                OrderId = orderId
            };

            return View(pvm);
        }


        [HttpPost]
        public async Task<IActionResult> Payment(Payment payment, string OrderId, double Price)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            Payment newPayment = new Payment();

            string PaymentId = Guid.NewGuid().ToString();

            newPayment.PaymentId = PaymentId;

            newPayment.OrderId = OrderId;
            newPayment.UserId = userId;
            newPayment.Amount = Price;
            newPayment.CardNumber = payment.CardNumber;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:7153/api/PaymentsMicroservice/");

                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage Response = await client.PostAsJsonAsync("AddPayment", newPayment);

                if (Response.IsSuccessStatusCode)
                {
                    using (var orderClient = new HttpClient())
                    {
                        orderClient.DefaultRequestHeaders.Clear();
                        orderClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        HttpResponseMessage OrderedResponse = await orderClient.GetAsync("https://localhost:7202/api/OrdersMicroservice/GetOrderDetails/" + OrderId);

                        if (OrderedResponse.IsSuccessStatusCode)
                        {

                            Order orderDetails = await OrderedResponse.Content.ReadAsAsync<Order>();

                            orderDetails.Paid = true;
                            orderDetails.Status = "Order received not yet dispatched";
                            orderDetails.PaymentId = PaymentId;

                            using (var updateOrderClient = new HttpClient())
                            {
                                updateOrderClient.DefaultRequestHeaders.Clear();
                                updateOrderClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                HttpResponseMessage UpdatedOrderedResponse = await updateOrderClient.PostAsJsonAsync("https://localhost:7202/api/OrdersMicroservice/update/", orderDetails);
                                if (UpdatedOrderedResponse.IsSuccessStatusCode)
                                {
                                    Shipment s = new Shipment();
                                    s.Status = "ordered received not yet dispatched";
                                    s.OrderId = OrderId;
                                    
                                    using (var ShipmentClient = new HttpClient())
                                    {
                                        ShipmentClient.DefaultRequestHeaders.Clear();
                                        ShipmentClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                        HttpResponseMessage response = await ShipmentClient.PostAsJsonAsync("https://localhost:7293/api/ShippingMicroservice/CreateShipment/",s);

                                        if (response.IsSuccessStatusCode)
                                        {
                                            return View("index");
                                        }

                                    }
                                    
                                }

                            }                            
                        }
                    }
                }
            }
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

                HttpResponseMessage Response = await client.GetAsync("https://localhost:7202/api/OrdersMicroservice/user/" + userId);
                if (Response.IsSuccessStatusCode)
                {
                    List<Order> orders = await Response.Content.ReadAsAsync<List<Order>>();
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

                HttpResponseMessage OrderedResponse = await orderClient.GetAsync("https://localhost:7202/api/OrdersMicroservice/GetOrderDetails/" + OrderId);

                if (OrderedResponse.IsSuccessStatusCode)
                {

                    Order orderDetails = await OrderedResponse.Content.ReadAsAsync<Order>();

                    orderDetails.ProductUrl = HttpUtility.UrlDecode(orderDetails.ProductUrl);

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

                HttpResponseMessage OrderedResponse = await orderClient.GetAsync("https://localhost:7153/api/PaymentsMicroservice/GetPaymentDetails/" + OrderId);

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

                HttpResponseMessage response = await client.GetAsync("https://localhost:7293/api/ShippingMicroservice/allShipments/");

                if (response.IsSuccessStatusCode)
                {

                    List<Shipment> shipments = await response.Content.ReadAsAsync<List<Shipment>>();



                    return View("GetShipments", shipments);
                }

            }
            return View("index");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateShipment(Shipment s)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage UpdatedOrderedResponse = await client.PostAsJsonAsync("https://localhost:7293/api/ShippingMicroservice/update/", s);
                if (UpdatedOrderedResponse.IsSuccessStatusCode)
                {
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

                HttpResponseMessage response = await client.PostAsJsonAsync("https://localhost:7293/api/ShippingMicroservice/GetShipmentDetails/", orderId);
                if (response.IsSuccessStatusCode)
                {
                    Shipment shipment = await response.Content.ReadAsAsync<Shipment>();
                    return View("UpdateShipment", shipment);
                }
            }
            return View("index");


        }

        //shipping and notifications


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
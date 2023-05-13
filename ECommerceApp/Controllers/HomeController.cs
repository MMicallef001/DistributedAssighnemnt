using Common.Models;
using ECommerceApp.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

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
                user.Id = "";
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
                    new Claim(ClaimTypes.Email, user.Email)
                    // Add other claims as needed.
                };

                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties();

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    return RedirectToAction("Index");
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
                    var response = await client.PostAsJsonAsync("https://localhost:7097/api/UserMicroservice/LoginUser/", credentials);

                    if (response.IsSuccessStatusCode)
                    {
                        // Create a session for the user.
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, email)

                            // Add other claims as needed.
                        };

                        var claimsIdentity = new ClaimsIdentity(
                            claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        var authProperties = new AuthenticationProperties();

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity),
                            authProperties);

                        return RedirectToAction("Index", "Home");
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
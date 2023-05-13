using Common.Models;
using customerMicroservice.DataAccess;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace customerMicroservice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserMicroservice : ControllerBase
    {

        private readonly FirestoreUsersRepo _context;

        public UserMicroservice(FirestoreUsersRepo context)
        {
            _context = context;
        }


        [HttpPost("RegisterUser")]
        public async Task<IActionResult> RegisterUser(User user)
        {

            user.Id = Guid.NewGuid().ToString();
            var check = _context.RegisterUser(user);

            if (check.Exception == null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Name)
                    // Add other claims as needed.
                };

                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties();

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return Ok("Registed Succesfully");
            }

            return NoContent();
        }

        [HttpPost("LoginUser")]
        public async Task<IActionResult> LoginUser(LogInModel user)
        {

            var check = await _context.Login(user.Email, user.Password);

            if (check)
            {
                return Ok();
            }
            return Problem("login Failed");
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<User>>> GetUserDetails(string id)
        {
            try
            {
                var userDetails = await _context.GetUserDetails(id);
                if (userDetails == null)
                {
                    return NotFound();
                }
                return Ok(userDetails);
            }
            catch
            {
                return NotFound();
            }
        }
    }
}


using customerMicroservice.DataAccess;
using customerMicroservice.Models;
using Google.Cloud.Firestore.V1;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

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

        [HttpPut()]
        public async Task<IActionResult> RegisterUser(User user)
        {

            user.Id = Guid.NewGuid().ToString();
            var check = _context.RegisterUser(user);

            if (check.Equals(true))
            {
                return Ok();
            }

            return NoContent();               
         }

        [HttpPost()]
        public async Task<IActionResult> LoginUser(string email, string password)
        {


            var check = await _context.Login(email, password);
        
            if (check)
            {
                return Ok();
            }
            return Problem("login Failed");
        }


        // GET: api/UserDetails/5
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
            catch {
                return NotFound();
            }
        }
    }
}


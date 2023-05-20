using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NotificationMicroservice.DataAccess;
using Common.Models;
using System.Text.Json;

namespace NotificationMicroservice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationMicroservice : ControllerBase
    {
        private readonly FirebaseNotificationRepo _context;

        public NotificationMicroservice(FirebaseNotificationRepo context)
        {
            _context = context;
        }


        [HttpPost("AddNotification")]
        public async Task<IActionResult> AddNotification(Notification n)
        {

            n.NotificationId = Guid.NewGuid().ToString();

            var check = _context.CreateNotification(n);

            if (check.Equals(true))
            {
                return Ok();
            }

            return NoContent();
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserNotification(string userId)
        {
            var list = await _context.GetNotifications(userId);

            string jsonString = JsonSerializer.Serialize(list);

            return Content(jsonString, "application/json");
        }
    }
}

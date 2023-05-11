using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ordersMicroservice.DataAccess;
using ordersMicroservice.Models;
using System.Text.Json;

namespace ordersMicroservice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersMicroservice : ControllerBase
    {
        private readonly FirebaseOrderRepo _context;

        public OrdersMicroservice(FirebaseOrderRepo context)
        {
            _context = context;
        }


        [HttpPost()]
        public async Task<IActionResult> AddOrder(Order order)
        {

            order.OrderId = Guid.NewGuid().ToString();

            var check = _context.CreateOrder(order);

            if (check.Equals(true))
            {
                return Ok();
            }

            return NoContent();
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserOrders(string userId)
        {
            var list = await _context.GetOrders(userId);

            string jsonString = JsonSerializer.Serialize(list);

            return Content(jsonString, "application/json");
        }

        [HttpGet("{orderId}")]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrderDetails(string orderId)
        {
            try
            {
                var orderDetails = await _context.GetOrderDetails(orderId);
                if (orderDetails == null)
                {
                    return NotFound();
                }
                return Ok(orderDetails);
            }
            catch
            {
                return NotFound();
            }
        }
    }
}

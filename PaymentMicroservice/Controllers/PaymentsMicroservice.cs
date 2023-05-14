using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PaymentMicroservice.DataAccess;
using Common.Models;

namespace PaymentMicroservice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsMicroservice : ControllerBase
    {
        private readonly FirebasePaymentsRepo _context;

        public PaymentsMicroservice(FirebasePaymentsRepo context)
        {
            _context = context;
        }

        [HttpPost("AddPayment")]
        public async Task<IActionResult> AddPayment(Payment payment)
        {

             var check = _context.CreatePayment(payment);

            if (check.Equals(true))
            {
                return Ok();
            }

            return NoContent();
        }

        [HttpGet("GetPaymentDetails/{orderId}")]
        public async Task<ActionResult<IEnumerable<Payment>>> GetPaymentDetails(string orderId)
        {
            try
            {
                var paymentDetails = await _context.GetPaymentDetails(orderId);
                if (paymentDetails == null)
                {
                    return NotFound();
                }
                return Ok(paymentDetails);
            }
            catch
            {
                return NotFound();
            }
        }

    }
}

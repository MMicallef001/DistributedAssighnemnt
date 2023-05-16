using Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShippingMicroservice.DataAccess;

namespace ShippingMicroservice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShippingMicroservice : ControllerBase
    {
        private readonly FirebaseShipingRepo _context;

        public ShippingMicroservice(FirebaseShipingRepo context)
        {
            _context = context;
        }


        [HttpPost("AddShipment")]
        public async Task<IActionResult> AddShipment(Shipment shipment)
        {
            var check = _context.CreateShipment(shipment);

            if (check.Equals(true))
            {
                return Ok();
            }

            return NoContent();
        }

        [HttpGet("GetShipmentDetails/{orderId}")]
        public async Task<ActionResult<IEnumerable<Shipment>>> GetShipmentDetails(string orderId)
        {
            try
            {
                var orderDetails = await _context.GetShipmentDetails(orderId);
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

        [HttpPost("update")]
        public async Task<ActionResult<IEnumerable<Shipment>>> Update(Shipment s)
        {
            try
            {
                var shipmentDetails = await _context.Update(s);
                if (shipmentDetails == null)
                {
                    return NotFound();
                }
                return Ok(shipmentDetails);
            }
            catch
            {
                return NotFound();
            }
        }
        [HttpGet("allShipments")]
        public async Task<ActionResult<IEnumerable<Shipment>>> GetShipments()
        {
            try
            {
                List<Shipment> shipments = await _context.GetShipments();
                if (shipments == null)
                {
                    return NotFound();
                }
                return Ok(shipments);
            }
            catch
            {
                return NotFound();
            }
        }
    }
}

using EzConDo_Service.Interface;
using EzConDo_Service.PayOsIntergration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace EzCondo_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "Resident")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            this.paymentService = paymentService;
        }

        [HttpPost("Create-QR-Payment")]
        public async Task<IActionResult> CreatePaymentForBooking([FromQuery] Guid bookingId)
        {
            var result = await paymentService.CreatePaymentForBookingAsync(bookingId);
            return Ok(result);
        }
    }
}

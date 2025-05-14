using EzCondo_Data.Context;
using EzConDo_Service.DTO;
using EzConDo_Service.Implement;
using EzConDo_Service.Interface;
using EzConDo_Service.PayOsIntergration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Net.payOS.Types;
using Azure;
using Net.payOS;
using System.Threading.Tasks;
using System.Security.Claims;

namespace EzCondo_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService paymentService;
        private readonly IConfiguration config;
        private readonly ApartmentDbContext dbContext;

        public PaymentController(IPaymentService paymentService, IConfiguration config, ApartmentDbContext dbContext)
        {
            this.paymentService = paymentService;
            this.config = config;
            this.dbContext = dbContext;
        }

        [Authorize(Policy = "Manager")]
        [HttpGet("History-Payment")]
        public async Task<IActionResult> GetAllPayments([FromQuery] string? search, int? month)
        {
            var result = await paymentService.GetAllPaymentsAsync(search, month);
            return Ok(result);
        }

        [Authorize(Policy = "Resident")]
        [HttpGet("My-History-Payment")]
        public async Task<IActionResult> GetMyPayments()
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new Exception("Token invalid");
            Guid.TryParse(userId, out var user_Id);
            var result = await paymentService.GetMyPaymentsAsync(user_Id);
            return Ok(result);
        }

        [Authorize(Policy = "Resident")]
        [HttpPost("Create-QR-Booking-Payment")]
        public async Task<IActionResult> CreatePaymentForBooking([FromQuery] Guid bookingId)
        {
            var result = await paymentService.CreatePaymentForBookingAsync(bookingId);
            return Ok(result);
        }

        [Authorize(Policy = "Resident")]
        [HttpPost("Create-QR-Electric-Payment")]
        public async Task<IActionResult> CreatePaymentForElectric([FromQuery] Guid electricBillId)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new Exception("Token invalid");
            Guid.TryParse(userId, out var user_Id);
            var result = await paymentService.CreatePaymentForElectricAsync(electricBillId, user_Id);
            return Ok(result);
        }

        [Authorize(Policy = "Resident")]
        [HttpPost("Create-QR-Water-Payment")]
        public async Task<IActionResult> CreatePaymentForWater([FromQuery] Guid waterBillId)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new Exception("Token invalid");
            Guid.TryParse(userId, out var user_Id);
            var result = await paymentService.CreatePaymentForWaterAsync(waterBillId, user_Id);
            return Ok(result);
        }

        [Authorize(Policy = "Resident")]
        [HttpPost("Create-QR-Parking-Payment")]
        public async Task<IActionResult> CreatePaymentForParking([FromQuery] Guid parkingId)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new Exception("Token invalid");
            Guid.TryParse(userId, out var user_Id);
            var result = await paymentService.CreatePaymentForParkingAsync(parkingId, user_Id);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook(WebhookType body)
        {
            bool result = await paymentService.HandleWebHookAsync(body);
            return Ok(result);
        }

        [Authorize(Policy = "Resident")]
        [HttpPost("Check-Payment-Status")]
        public async Task<IActionResult> CheckPaymentStatus([FromQuery] Guid paymentId)
        {
            return Ok(await paymentService.CheckPaymentAsync(paymentId));
        }
    }
}

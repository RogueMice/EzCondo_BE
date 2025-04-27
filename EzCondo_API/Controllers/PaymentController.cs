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
using static EzConDo_Service.ExceptionsConfig.CustomException;
using System.Text.Json;
using Net.payOS.Types;
using Azure;
using Net.payOS;
using System.Threading.Tasks;

namespace EzCondo_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "Resident")]
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
        [HttpPost("Create-QR-Payment")]
        public async Task<IActionResult> CreatePaymentForBooking([FromQuery] Guid bookingId)
        {
            var result = await paymentService.CreatePaymentForBookingAsync(bookingId);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("Web-hook")]
        public async Task<IActionResult> Webhook(WebhookType body)
        {
            bool result = await paymentService.HandleWebHookAsync(body);
            return Ok(result);
        }

        [HttpPost("Check-Payment-Status")]
        public async Task<IActionResult> CheckPaymentStatus([FromQuery] Guid paymentId)
        {
            return Ok(await paymentService.CheckPaymentAsync(paymentId));
        }
    }
}

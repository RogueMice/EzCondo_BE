using EzConDo_Service.DTO;
using EzConDo_Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EzCondo_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService bookingService;

        public BookingController(IBookingService bookingService)
        {
            this.bookingService = bookingService;
        }

        [Authorize(Policy = "Manager")]
        [HttpGet("Get-All-Booking")]
        public async Task<IActionResult> GetAllBooking([FromQuery] string? search, int? month)
        {
            var result = await bookingService.GetAllBookingsAsync(search, month);
            return Ok(result);
        }

        [Authorize(Policy = "Resident")]
        [HttpPost("Add-Booking")]
        public async Task<IActionResult> AddOrUpdateBooking([FromBody] BookingDTO dto)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();
            Guid.TryParse(userId, out var user_Id);
            dto.UserId = user_Id;
            var result = await bookingService.AddBookingAsync(dto);
            return Ok(result);
        }
    }
}

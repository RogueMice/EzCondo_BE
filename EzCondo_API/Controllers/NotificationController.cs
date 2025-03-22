using EzConDo_Service.DTO;
using EzConDo_Service.Implement;
using EzConDo_Service.Interface;
using EzConDo_Service.SignalR_Integration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading;

namespace EzCondo_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService notificationService;

        public NotificationController(INotificationService notificationService)
        {
            this.notificationService = notificationService;
        }

        [HttpGet("user-get-notifications")]
        public async Task<IActionResult> GetNotifications([FromQuery] bool isRead, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();
            Guid.TryParse(userId, out var user_Id);
            var result = await notificationService.GetNotificationsAsync(isRead, page, pageSize, user_Id);
            if (result == null)
                return BadRequest();
            return Ok(result);
        }

        [HttpPost("notifications/mark-as-read")]
        public async Task<IActionResult> MarkAsRead([FromBody] MarkAsReadRequestDTO request)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();
            Guid.TryParse(userId, out var user_Id);
            var result = await notificationService.MarkAsReadAsync(request, user_Id);
            if (result == null)
                return BadRequest();
            return Ok(result);
        }

        [Authorize(Policy = "Admin")]
        [HttpGet("admin-get-notifications")]
        public async Task<IActionResult> GetAllNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] int? day = 7)
        {
            var result = await notificationService.AdminGetNotificationsAsync(page,pageSize,day);
            if (result == null)
                return BadRequest();
            return Ok(result);
        }
    }
}

﻿using EzConDo_Service.DTO;
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
        private readonly INotificationImageService imageService;
        private readonly IHubContext<NotificationHub> hubContext;

        public NotificationController(INotificationService notificationService, INotificationImageService imageService, IHubContext<NotificationHub> hubContext)
        {
            this.notificationService = notificationService;
            this.imageService = imageService;
            this.hubContext = hubContext;
        }

        [HttpGet("user-get-notifications")]
        public async Task<IActionResult> GetNotifications([FromQuery] bool? isRead, [FromQuery] string? type, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();
            Guid.TryParse(userId, out var user_Id);
            var result = await notificationService.GetNotificationsAsync(isRead, page, pageSize, user_Id,type);
            if (result == null)
                return BadRequest();
            return Ok(result);
        }

        [Authorize(Policy = "AdminOrManager")]
        [HttpGet("admin-or-manager-get-notifications")]
        public async Task<IActionResult> GetAllNotifications([FromQuery] string? type,[FromQuery] string? receiver, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] int? day = 7)
        {
            var result = await notificationService.AdminGetNotificationsAsync(page, pageSize,day,receiver,type);
            if (result == null)
                return BadRequest();
            return Ok(result);
        }


        [HttpGet("get-notification-images-by-id")]
        public async Task<IActionResult> GetNotificationImagesById([FromQuery] Guid notificationId)
        {
            var notification = await imageService.GetNotificationImageAsync(notificationId);
            return Ok(notification);
        }

        [Authorize(Policy = "AdminOrManager")]
        [HttpPost("create-notification")]
        public async Task<IActionResult> CreateNotification(CreateNotificationDTO createNotificationDTO)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();
            Guid.TryParse(userId, out var user_Id);

            var notification = await notificationService.CreateNotificationAsync(createNotificationDTO, user_Id);
            if (notification == null)
                return BadRequest("Create Notification failure");
            return Ok(notification);
        }

        [Authorize(Policy = "AdminOrManager")]
        [HttpPost("create-notification-images")]
        public async Task<IActionResult> CreateNotificationImages([FromForm] NotificationImageDTO dto)
        {
            var notification = await imageService.CreateNotificationImageAsync(dto);
            if (notification == null)
                return BadRequest("Create Notification failure");
            return Ok(notification);
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

        [Authorize(Policy = "Manager")]
        [HttpPost("manager-send-to-user")]
        public async Task<IActionResult> SendNotificationToUser([FromBody] List<SendNotificationToUserDTO> dto)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();
            Guid.TryParse(userId, out var user_Id);
            var result = await notificationService.CreateNotificationsToUsersAsync(dto, user_Id);
            if (result == null)
                return BadRequest();
            return Ok(result);
        }

        [Authorize(Policy = "Resident")]
        [HttpPost("resident-send-feedback")]
        public async Task<IActionResult> SendFeedback([FromBody] CreateNotificationDTO dto)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();
            Guid.TryParse(userId, out var user_Id);
            var result = await notificationService.UserCreateNotificationAsync(dto, user_Id);
            if (result == null)
                return BadRequest();
            return Ok(result);
        }
    }
}

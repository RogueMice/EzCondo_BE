using EzCondo_Data.Context;
using EzConDo_Service.DTO;
using EzConDo_Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service.IService;
using System.Security.Claims;

namespace EzCondo_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService userService;
        private readonly IUserDeviceService userDeviceService;

        public UserController(IUserService userService, IUserDeviceService userDeviceService)
        {
            this.userService = userService;
            this.userDeviceService = userDeviceService;
        }

        [HttpGet("get-infor-me")]
        public async Task<IActionResult> GetMe()
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new Exception("Token invalid");
            Guid.TryParse(userId, out var user_Id);
            var user = await userService.GetCurrentUserInfoAsync(user_Id) ?? throw new Exception("User not found");
            return Ok(user);
        }

        [HttpPatch("edit-infor-me")]
        public async Task<IActionResult> EditMe(EditUserDTO userDTO)
        {
            userDTO.Id = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new Exception("Token invalid"));
            var user = await userService.EditCurrentUserInforAsync(userDTO);
            return Ok(user);
        }


        [HttpPost("add-or-update-avatar")]
        public async Task<IActionResult> AddOrUpdateAvatar(IFormFile avatar)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) 
                return Unauthorized();
            Guid.TryParse(userId, out var user_Id);
            var result = await userService.AddOrUpdateAvtAsync(user_Id, avatar);
            if (!result)
                return BadRequest("Something went wrong !");
            return Ok("Avatar updated successfully!");
        }


        [HttpPatch("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO dto)
        {
            dto.UserId = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new Exception("Token invalid"));
            var user = await userService.ChangePasswordAsync(dto);
            return Ok(user);
        }

        [HttpPost("update-fcm-token")]
        public async Task<IActionResult> UpdateFcmToken([FromBody] UpdateFcmTokenDTO dto)
        {
            dto.UserId = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new Exception("Token invalid"));
            var result = await userDeviceService.UpdateFcmToken(dto);
            if (result == null)
                return BadRequest("Something went wrong !");
            return Ok(result);
        }
    }
}

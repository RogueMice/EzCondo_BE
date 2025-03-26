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

        [Authorize(Policy = "Admin")]
        [HttpGet("get-all-users")]
        public async Task<IActionResult> GetAll(string? roleName, string? search)
        {
            var users = await userService.GetUsersAsync(roleName, search);
            if (users == null)
                return BadRequest("Don't have any user !");
            return Ok(users);
        }

        [Authorize(Policy = "AdminOrManager")]
        [HttpGet("get-user-by-id")]
        public async Task<IActionResult> GetUserById(Guid userId)
        {
            var users = await userService.GetUserByIdDTOAsync(userId);
            if (users == null)
                return BadRequest("Don't have any user !");
            return Ok(users);
        }

        [HttpGet("get-infor-me")]
        public async Task<IActionResult> GetMe()
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new Exception("Token invalid");
            Guid.TryParse(userId, out var user_Id);
            var user = await userService.GetCurrentUserInfoAsync(user_Id) ?? throw new Exception("User not found");
            return Ok(user);
        }

        [Authorize(Policy = "Admin")]
        [HttpPost("add-user")]
        public async Task<IActionResult> AddUser([FromBody] AddUserDTO userDTO)
        {
            var user = await userService.AddUserAsync(userDTO);
            if (user == Guid.Empty)
                return BadRequest("Add User is failure !");
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

        [Authorize(Policy = "AdminOrManager")]
        [HttpPatch("update-user")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDTO userDTO)
        {
            var user = await userService.UpdateUserAsync(userDTO);
            if (user == Guid.Empty)
                return BadRequest("Update User is failure !");
            return Ok(user);
        }

        [HttpPatch("edit-infor-me")]
        public async Task<IActionResult> EditMe(EditUserDTO userDTO)
        {
            userDTO.Id = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new Exception("Token invalid"));
            var user = await userService.EditCurrentUserInforAsync(userDTO);
            return Ok(user);
        }

        [HttpPatch("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO dto)
        {
            dto.UserId = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new Exception("Token invalid"));
            var user = await userService.ChangePasswordAsync(dto);
            return Ok(user);
        }

        [Authorize(Policy = "Admin")]
        [HttpDelete("delete-user-by-id")]
        public async Task<IActionResult> DeleteUserById(Guid userId)
        {
            var user = await userService.DeleteUserAsync(userId);
            if (user == Guid.Empty)
                return BadRequest("Delete User is failure !");
            return Ok(user);
        }
    }
}

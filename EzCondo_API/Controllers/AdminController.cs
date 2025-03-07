using EzCondo_Data.Context;
using EzConDo_Service.DTO;
using EzConDo_Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service.IService;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace EzCondo_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IUserService userService;
        private readonly ICitizenService citizenService;
        private readonly ApartmentDbContext dbContext;

        public AdminController(IUserService userService, ICitizenService citizenService, ApartmentDbContext dbContext)
        {
            this.userService = userService;
            this.citizenService = citizenService;
            this.dbContext = dbContext;
        }

        [HttpGet("Get-All-Users")]
        public async Task<IActionResult> GetAll(string? roleName)
        {
            var users = await userService.GetUsersAsync(roleName);
            if (users == null)
                return BadRequest("Don't have any user !");
            return Ok(users);
        }

        [HttpPost("Add-User")]
        public async Task<IActionResult> AddUser([FromBody] AddUserDTO userDTO)
        {
            var user = await userService.AddUserAsync(userDTO);
            if (user == null)
                return BadRequest("Add User is failure !");
            return Ok(user);
        }


        [HttpPost("Add-Or-Update-Citizen")]
        public async Task<IActionResult> AddOrUpdateCitizen([FromForm] CitizenDTO citizenDTO)
        {
            var citizen = await citizenService.AddOrUpdateCitizenAsync(citizenDTO);
            if (citizen == null)
                return BadRequest("Add or Update citizen is failure !");
            return Ok("Success !!");
        }

        [HttpPatch("Update-User")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDTO userDTO)
        {
            var user = await userService.UpdateUserAsync(userDTO);
            if (user == null)
                return BadRequest("Update User is failure !");
            return Ok(user);
        }

        [HttpDelete("Delete-User-By-Id")]
        public async Task<IActionResult> DeleteUserById(Guid userId)
        {
            var user = await userService.DeleteUserAsync(userId);
            if (user == null)
                return BadRequest("Delete User is failure !");
            return Ok(user);
        }
    }
}

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
        private readonly IService_service service_service;
        private readonly IService_ImageService service_ImageService;

        public AdminController(IUserService userService, 
                                ICitizenService citizenService, 
                                ApartmentDbContext dbContext, 
                                IService_service service_Service,
                                IService_ImageService service_ImageService)
        {
            this.userService = userService;
            this.citizenService = citizenService;
            this.dbContext = dbContext;
            this.service_service = service_Service;
            this.service_ImageService = service_ImageService;
        }

        [HttpGet("get-all-users")]
        public async Task<IActionResult> GetAll(string? roleName, string? search)
        {
            var users = await userService.GetUsersAsync(roleName, search);
            if (users == null)
                return BadRequest("Don't have any user !");
            return Ok(users);
        }

        [HttpPost("add-user")]
        public async Task<IActionResult> AddUser([FromBody] AddUserDTO userDTO)
        {
            var user = await userService.AddUserAsync(userDTO);
            if (user == null)
                return BadRequest("Add User is failure !");
            return Ok(user);
        }


        [HttpPost("add-or-update-citizen")]
        public async Task<IActionResult> AddOrUpdateCitizen([FromForm] CitizenDTO citizenDTO)
        {
            var citizen = await citizenService.AddOrUpdateCitizenAsync(citizenDTO);
            if (citizen == null)
                return BadRequest("Add or Update citizen is failure !");
            return Ok("Success !!");
        }

        [HttpPatch("update-user")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDTO userDTO)
        {
            var user = await userService.UpdateUserAsync(userDTO);
            if (user == null)
                return BadRequest("Update User is failure !");
            return Ok(user);
        }

        [HttpDelete("delete-user-by-id")]
        public async Task<IActionResult> DeleteUserById(Guid userId)
        {
            var user = await userService.DeleteUserAsync(userId);
            if (user == null)
                return BadRequest("Delete User is failure !");
            return Ok(user);
        }

        [HttpPost("add-service")]
        public async Task<IActionResult> AddService([FromBody] AddServiceDTO serviceDTO)
        {
            var service = await service_service.AddServiceAsync(serviceDTO);
            if (service == null)
                return BadRequest("Add service is failure !");
            return Ok(service);
        }

        [HttpPost("add-service-images")]
        public async Task<IActionResult> AddServiceImages([FromForm] Service_ImageDTO serviceImageDTO)
        {
            await service_ImageService.AddServiceImagesAsync(serviceImageDTO);
            return Ok("Add service images is successful !");
        }
    }
}

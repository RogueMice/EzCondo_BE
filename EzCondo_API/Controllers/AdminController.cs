using EzCondo_Data.Context;
using EzConDo_Service.DTO;
using EzConDo_Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service.IService;
using System.Security.Claims;
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
        private readonly INotificationService notificationService;

        public AdminController(IUserService userService, 
                                ICitizenService citizenService, 
                                ApartmentDbContext dbContext, 
                                IService_service service_Service,
                                IService_ImageService service_ImageService,
                                INotificationService notificationService)
        {
            this.userService = userService;
            this.citizenService = citizenService;
            this.dbContext = dbContext;
            this.service_service = service_Service;
            this.service_ImageService = service_ImageService;
            this.notificationService = notificationService;
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
            if (user == Guid.Empty)
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
            if (user == Guid.Empty)
                return BadRequest("Update User is failure !");
            return Ok(user);
        }

        [HttpDelete("delete-user-by-id")]
        public async Task<IActionResult> DeleteUserById(Guid userId)
        {
            var user = await userService.DeleteUserAsync(userId);
            if (user == Guid.Empty)
                return BadRequest("Delete User is failure !");
            return Ok(user);
        }

        [HttpPost("add-or-update-service")]
        public async Task<IActionResult> AddService([FromBody] AddServiceDTO serviceDTO)
        {
            var service = await service_service.AddOrUpdateServiceAsync(serviceDTO);
            if (service == Guid.Empty)
                return BadRequest("Add service is failure !");
            return Ok(service);
        }

        [HttpPost("add-or-update-service-images")]
        public async Task<IActionResult> AddOrUpdateServiceImages([FromForm] Service_ImageDTO serviceImageDTO)
        {
            await service_ImageService.AddOrUpdateServiceImagesAsync(serviceImageDTO);
            return Ok("AddOrUpdate service images is successful !");
        }

        [HttpGet("get-all-citizens")]
        public async Task<IActionResult> GetAllCitizens()
        {
            var citizen = await citizenService.GetAllCitizensAsync();
            if (citizen != null)
                return Ok(citizen);
            return null; 
        }

        [HttpGet("get-user-by-id")]
        public async Task<IActionResult> GetUserById(Guid userId)
        {
            var users = await userService.GetUserByIdDTOAsync(userId);
            if (users == null)
                return BadRequest("Don't have any user !");
            return Ok(users);
        }

        [HttpPost("create-notification")]
        public async Task<IActionResult> CreateNotification(CreateNotificationDTO createNotificationDTO)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();
            Guid.TryParse(userId, out var user_Id);

            var notification = await notificationService.CreateNotificationAsync(createNotificationDTO,user_Id);
            if (notification == null)
                return BadRequest("Create Notification failure");
            return Ok(notification);
        }
    }
}

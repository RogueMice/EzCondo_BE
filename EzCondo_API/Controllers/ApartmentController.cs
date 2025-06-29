﻿using EzConDo_Service.DTO;
using EzConDo_Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EzCondo_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApartmentController : ControllerBase
    {
        private readonly IApartmentService apartmentService;

        public ApartmentController(IApartmentService apartmentService)
        {
            this.apartmentService = apartmentService;
        }

        [Authorize(Policy = "AdminOrManager")]
        [HttpGet("apartment-dashboard")]
        public async Task<IActionResult> GetApartmentDashboard()
        {
            var apartments = await apartmentService.GetApartmentDashBoardAsync();
            return Ok(apartments);
        }


        [Authorize(Policy = "Resident")]
        [HttpGet("get-my-apartment")]
        public async Task<IActionResult> GetMyApartment()
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();
            Guid.TryParse(userId, out var user_Id);
            var apartment = await apartmentService.GetMyApartmentAsync(user_Id);
            return Ok(apartment);
        }

        [Authorize(Policy = "AdminOrManager")]
        [HttpGet("get-all-apartment")]
        public async Task<IActionResult> GetAllApartment([FromQuery] Guid? userId, [FromQuery] string? apartmentNumber,[FromQuery] bool? status)
        {
            var apartments = await apartmentService.GetApartmentAsync(userId,apartmentNumber,status);
            return Ok(apartments);
        }

        [Authorize(Policy = "AdminOrManager")]
        [HttpGet("get-apartment-by-id")]
        public async Task<IActionResult> GetApartmentById([FromQuery] Guid apartmentId)
        {
            var apartments = await apartmentService.GetApartmentByIdAsync(apartmentId);
            return Ok(apartments);
        }

        [Authorize(Policy = "AdminOrManager")]
        [HttpPost("add-apartment")]
        public async Task<IActionResult> AddApartment([FromBody] ApartmentViewDTO dto)
        {
            var apartments = await apartmentService.AddApartmentAsync(dto);
            return Ok(apartments);
        }

        [Authorize(Policy = "AdminOrManager")]
        [HttpPatch("update-apartment")]
        public async Task<IActionResult> UpdateApartment([FromBody] ApartmentUpdateDTO dto)
        {
            var result = await apartmentService.UpdateApartmentAsync(dto);
            return Ok(result);
        }
    }
}

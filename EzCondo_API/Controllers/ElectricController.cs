﻿using EzConDo_Service.DTO;
using EzConDo_Service.Implement;
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
    public class ElectricController : ControllerBase
    {
        private readonly IElectricService electricService;

        public ElectricController(IElectricService electricService)
        {
            this.electricService = electricService;
        }

        [Authorize(Policy = "Manager")]
        [HttpGet("Get-All-Electric-Metters")]
        public async Task<IActionResult> GetAllElectricMetters()
        {
            var electricMetter = await electricService.GetAllElectricMettersAsync();
            return Ok(electricMetter);
        }


        [Authorize(Policy = "Manager")]
        [HttpGet("Get-All-Electric-Readings")]
        public async Task<IActionResult> GetAllElectricReadings()
        {
            var electricReading = await electricService.GetAllElectricReadingsAsync();
            return Ok(electricReading);
        }

        [Authorize(Policy = "AdminOrManager")]
        [HttpGet("Get-All-Electric")]
        public async Task<IActionResult> GetAllElectric([FromQuery] bool? status, [FromQuery] int? day, [FromQuery] int? month)
        {
            var electric = await electricService.GetAllElectricAsync(status,day,month);
            return Ok(electric);
        }

        [Authorize(Policy = "Manager")]
        [HttpGet("Get-Electric-Detail")]
        public async Task<IActionResult> GetElectricDetail([FromQuery] Guid electricId)
        {
            var electricDetail = await electricService.GetElectricDetailAsync(electricId);
            return Ok(electricDetail);
        }

        [HttpGet("Get-My-Electric-Detail")]
        public async Task<IActionResult> GetMyElectricDetail([FromQuery] bool? status)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();
            Guid.TryParse(userId, out var user_Id);
            var electricDetail = await electricService.GetMyElectricDetailAsync(user_Id, status);
            return Ok(electricDetail);
        }

        [Authorize(Policy = "Manager")]
        [HttpGet("Download-Template-Electric-Metter")]
        public async Task<IActionResult> CreateTemplateElectricMeter()
        {
            var content = await electricService.CreateTemplateElectricMetterAsync();
            return File(content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "ElectricMetersTemplate.xlsx");
        }

        [Authorize(Policy = "Manager")]
        [HttpGet("Download-Template-Electric-Reading")]
        public async Task<IActionResult> CreateTemplateElectricReading()
        {
            var content = await electricService.CreateTemplateElectricReadingAsync();
            return File(content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "ElectricReadingsTemplate.xlsx");
        }

        [Authorize(Policy = "Manager")]
        [HttpPost("Add-Electric-Metters")]
        public async Task<IActionResult> AddElectricMetters( IFormFile file)
        {
            var electricMetter = await electricService.AddElectricMettersAsync(file);
            if (electricMetter == null)
            {
                return BadRequest("Don't have any data imported !");
            }
            return Ok(new
            {
                message = "Add successful !",
                data = electricMetter
            });
        }

        [Authorize(Policy = "Manager")]
        [HttpPost("Add-Electric-Readings")]
        public async Task<IActionResult> AddElectricReadings(IFormFile file)
        {
            var electricReading = await electricService.AddElectricReadingAsync(file);
            if (electricReading == null)
            {
                return BadRequest("Don't have any data imported !");
            }
            return Ok(new
            {
                message = "Add successful !",
                data = electricReading
            });
        }

        [Authorize(Policy = "Manager")]
        [HttpPatch("Update-Electric-Bill")]
        public async Task<IActionResult> UpdateElectricBills(List<UpdateElectricBillDTO> dtos)
        {
            var electricBills = await electricService.UpdateElectricBillsAsync(dtos);
            return Ok(electricBills);
        }
    }
}

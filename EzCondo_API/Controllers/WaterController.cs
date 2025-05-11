using EzConDo_Service.DTO;
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
    public class WaterController : ControllerBase
    {
        private readonly IWaterService waterService;

        public WaterController(IWaterService waterService)
        {
            this.waterService = waterService;
        }

        [Authorize(Policy = "Manager")]
        [HttpGet("Get-All-Water-Metters")]
        public async Task<IActionResult> GetAllWaterMetters()
        {
            var waterMetter = await waterService.GetAllWaterMettersAsync();
            return Ok(waterMetter);
        }

        [Authorize(Policy = "Manager")]
        [HttpGet("Get-All-Water-Readings")]
        public async Task<IActionResult> GetAllWaterReadings()
        {
            var waterReading = await waterService.GetAllWaterReadingsAsync();
            return Ok(waterReading);
        }

        [Authorize(Policy = "Manager")]
        [HttpGet("Get-All-Water")]
        public async Task<IActionResult> GetAllWater([FromQuery] bool? status, [FromQuery] int? day, [FromQuery] int? month)
        {
            var water = await waterService.GetAllWaterAsync(status, day, month);
            return Ok(water);
        }

        [Authorize(Policy = "Manager")]
        [HttpGet("Get-Water-Detail")]
        public async Task<IActionResult> GetWaterDetail([FromQuery] Guid waterReadingId)
        {
            var waterDetail = await waterService.GetWaterDetailAsync(waterReadingId);
            return Ok(waterDetail);
        }

        [HttpGet("Get-My-Electric-Detail")]
        public async Task<IActionResult> GetMyElectricDetail([FromQuery] bool? status)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();
            Guid.TryParse(userId, out var user_Id);
            var waterDetail = await waterService.GetMyWaterDetailAsync(user_Id, status);
            return Ok(waterDetail);
        }

        [Authorize(Policy = "Manager")]
        [HttpGet("Download-Template-Water-Metter")]
        public async Task<IActionResult> CreateTemplateWaterMeter()
        {
            var content = await waterService.CreateTemplateWaterMetterAsync();
            return File(content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "WaterMettersTemplate.xlsx");
        }

        [Authorize(Policy = "Manager")]
        [HttpGet("Download-Template-Water-Reading")]
        public async Task<IActionResult> CreateTemplateWaterReading()
        {
            var content = await waterService.CreateTemplateWaterReadingAsync();
            return File(content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "WaterReadingTemplate.xlsx");
        }

        [Authorize(Policy = "Manager")]
        [HttpPost("Add-Water-Metter")]
        public async Task<IActionResult> AddWaterMetters(IFormFile file)
        {
            var waterMetters = await waterService.AddWaterMettersAsync(file);
            return Ok(waterMetters);
        }

        [Authorize(Policy = "Manager")]
        [HttpPost("Add-Water-Reading")]
        public async Task<IActionResult> AddWaterReading(IFormFile file)
        {
            var waterReadings = await waterService.AddWaterReadingAsync(file);
            return Ok(waterReadings);
        }

        [Authorize(Policy = "Manager")]
        [HttpPatch("Update-Water-Bill")]
        public async Task<IActionResult> UpdateWaterBills(List<UpdateWaterBillDTO> dtos)
        {
            var waterBills = await waterService.UpdateWaterBillsAsync(dtos);
            return Ok(waterBills);
        }
    }
}

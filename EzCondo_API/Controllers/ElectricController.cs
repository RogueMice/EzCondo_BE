using EzConDo_Service.DTO;
using EzConDo_Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EzCondo_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ElectricController : ControllerBase
    {
        private readonly IElectricMeterService meterService;

        public ElectricController(IElectricMeterService meterService)
        {
            this.meterService = meterService;
        }

        [HttpGet("Get-All-Electric-Metters")]
        public async Task<IActionResult> GetAllElectricMetters()
        {
            var electricMetter = await meterService.GetAllElectricMettersAsync();
            return Ok(electricMetter);
        }

        [HttpGet("Get-All-Electric-Readings")]
        public async Task<IActionResult> GetAllElectricReadings()
        {
            var electricReading = await meterService.GetAllElectricReadingsAsync();
            return Ok(electricReading);
        }

        [Authorize(Policy = "Manager")]
        [HttpPost("Add-Electric-Metters")]
        public async Task<IActionResult> AddElectricMetters( IFormFile file)
        {
            var electricMetter = await meterService.AddElectricMettersAsync(file);
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

        [Authorize(Policy = "AdminOrManager")]
        [HttpPost("Add-Electric-Readings")]
        public async Task<IActionResult> AddElectricReadings(IFormFile file)
        {
            var electricReading = await meterService.AddElectricReadingAsync(file);
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
    }
}

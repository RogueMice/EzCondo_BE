using EzConDo_Service.DTO;
using EzConDo_Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EzCondo_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "Manager")]
    public class ElectricController : ControllerBase
    {
        private readonly IElectricService electricService;

        public ElectricController(IElectricService electricService)
        {
            this.electricService = electricService;
        }

        [HttpGet("Get-All-Electric-Metters")]
        public async Task<IActionResult> GetAllElectricMetters()
        {
            var electricMetter = await electricService.GetAllElectricMettersAsync();
            return Ok(electricMetter);
        }

        [HttpGet("Get-All-Electric-Readings")]
        public async Task<IActionResult> GetAllElectricReadings()
        {
            var electricReading = await electricService.GetAllElectricReadingsAsync();
            return Ok(electricReading);
        }

        [HttpGet("Get-All-Electric")]
        public async Task<IActionResult> GetAllElectric([FromQuery] bool? status, [FromQuery] int? day = 30)
        {
            var electric = await electricService.GetAllElectricAsync(status,day);
            return Ok(electric);
        }


        [HttpGet("Get-Electric-Detail")]
        public async Task<IActionResult> GetElectricDetail([FromQuery] Guid electricId)
        {
            var electricDetail = await electricService.GetElectricDetailAsync(electricId);
            return Ok(electricDetail);
        }

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
    }
}

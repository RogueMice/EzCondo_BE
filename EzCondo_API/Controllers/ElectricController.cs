using EzConDo_Service.DTO;
using EzConDo_Service.Interface;
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

        [HttpPost("Add-Electric-Metters")]
        public async Task<IActionResult> AddElectricMetters( IFormFile file)
        {
            var electricMetter = await meterService.AddElectricMetters(file);
            if (electricMetter == null)
            {
                return BadRequest("Không có dữ liệu nào được thêm vào");
            }
            return Ok(new
            {
                message = "Thêm thành công",
                data = electricMetter
            });
        }
    }
}

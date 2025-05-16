using EzConDo_Service.DTO;
using EzConDo_Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EzCondo_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OtherServiceController : ControllerBase
    {
        private readonly IOtherService otherService;

        public OtherServiceController(IOtherService otherService)
        {
            this.otherService = otherService;
        }

        [HttpGet("get-all-other-service")]
        public async Task<IActionResult> GetAllOtherService()
        {
            var result = await otherService.GetOtherServiceAsync();
            return Ok(result);
        }

        [Authorize(Policy = "Admin")]
        [HttpPost("add-or-update-other-service")]
        public async Task<IActionResult> AddOrUpdateOtherService([FromBody] OtherServiceDTO dto)
        {
            var result = await otherService.AddOrUpdateAsync(dto);
            return Ok(result);
        }

        [Authorize(Policy = "Admin")]
        [HttpDelete("delete-other-service")]
        public async Task<IActionResult> DeleteOtherService([FromQuery] Guid otherServiceId)
        {
            var result = await otherService.DeleteOtherServiceAsync(otherServiceId);
            return Ok(result);
        }
    }
}

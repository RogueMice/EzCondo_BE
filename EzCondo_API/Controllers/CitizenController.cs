using EzConDo_Service.DTO;
using EzConDo_Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EzCondo_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CitizenController : ControllerBase
    {
        private readonly ICitizenService citizenService;

        public CitizenController(ICitizenService citizenService)
        {
            this.citizenService = citizenService;
        }

        [Authorize(Policy = "AdminOrManager")]
        [HttpGet("get-all-citizens")]
        public async Task<IActionResult> GetAllCitizens()
        {
            var citizen = await citizenService.GetAllCitizensAsync();
            if (citizen != null)
                return Ok(citizen);
            return null;
        }

        [Authorize(Policy = "AdminOrManager")]
        [HttpPost("add-or-update-citizen")]
        public async Task<IActionResult> AddOrUpdateCitizen([FromForm] CitizenDTO citizenDTO)
        {
            var citizen = await citizenService.AddOrUpdateCitizenAsync(citizenDTO);
            if (citizen == null)
                return BadRequest("Add or Update citizen is failure !");
            return Ok("Success !!");
        }


    }
}

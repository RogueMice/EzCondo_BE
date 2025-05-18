using EzConDo_Service.DTO;
using EzConDo_Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EzCondo_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IncidentController : ControllerBase
    {
        private readonly I_incidentService i_IncidentService;
        private readonly I_IncidentImage i_IncidentImage;

        public IncidentController(I_incidentService i_IncidentService, I_IncidentImage i_IncidentImage)
        {
            this.i_IncidentService = i_IncidentService;
            this.i_IncidentImage = i_IncidentImage;
        }

        [Authorize(Policy = "AdminOrManager")]
        [HttpGet("incident-dash-board")]
        public async Task<IActionResult> GetIncidentDashBoard()
        {
            var incidents = await i_IncidentService.GetIncidentDashboardAsync();
            return Ok(incidents);
        }

        [Authorize(Policy = "Manager")]
        [HttpGet("get-all-incident")]
        public async Task<IActionResult> GetAllIncident()
        {
            var incidents = await i_IncidentService.GetIncidentsAsync();
            return Ok(incidents);
        }

        [Authorize(Policy = "Manager")]
        [HttpGet("get-incident-by-id")]
        public async Task<IActionResult> GetIncidentById([FromQuery] Guid incidentId)
        {
            var incident = await i_IncidentService.GetIncidentByIdAsync(incidentId);
            return Ok(incident);
        }

        [Authorize(Policy = "Resident")]
        [HttpGet("get-my-incident")]
        public async Task<IActionResult> GetMyIncident()
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();
            Guid.TryParse(userId, out var user_Id);
            var incidents = await i_IncidentService.GetIncidentByUserIdAsync(user_Id);
            return Ok(incidents);
        }

        [Authorize]
        [HttpGet("get-incident-image-by-incident-id")]
        public async Task<IActionResult> GetIncidentByIncidentId([FromQuery] Guid incidentId)
        {
            var incident = await i_IncidentImage.GetIncidentImageByIdAsync(incidentId);
            return Ok(incident);
        }

        [Authorize(Policy = "Resident")]
        [HttpPost("add-incident")]
        public async Task<IActionResult> AddIncident([FromBody] IncidentDTO incidentDto)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) 
                return Unauthorized();
            Guid.TryParse(userId, out var user_Id);
            var incident = await i_IncidentService.AddAsync(incidentDto, user_Id);
            if (incident == null)
                return BadRequest("Add incident is failure !");
            return Ok(incident);
        }

        [Authorize(Policy = "Resident")]
        [HttpPost("add-or-update-incident-image")]
        public async Task<IActionResult> AddOrUpdateIncidentImage([FromForm] IncidentImageDTO incidentImageDto)
        {
            var incidentImage = await i_IncidentImage.AddOrUpdateIncidentImageAsync(incidentImageDto);
            return Ok(incidentImage);
        }

        [Authorize(Policy = "Manager")]
        [HttpPatch("update-incident-status")]
        public async Task<IActionResult> UpdateIncidentStatus([FromBody] UpdateIncidentDTO incidentUpdateDto)
        {
            var incident = await i_IncidentService.EditAsync(incidentUpdateDto);
            if (incident == null)
                return BadRequest("Update incident is failure !");
            return Ok(incident);
        }
    }
}

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

    public class HouseHoldMemberController : ControllerBase
    {
        private readonly IHouseHoldMemberService houseHoldMemberService;

        public HouseHoldMemberController(IHouseHoldMemberService houseHoldMemberService)
        {
            this.houseHoldMemberService = houseHoldMemberService;
        }

        [Authorize(Policy = "Resident")]
        [HttpGet("get-my-house-hold-member")]
        public async Task<IActionResult> GetMyHoldHouseMember()
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();
            Guid.TryParse(userId, out var user_Id);
            var houseHoldMember = await houseHoldMemberService.GetMyHoldHouseMemberAsync(user_Id);
            return Ok(houseHoldMember);
        }

        [Authorize(Policy = "AdminOrManager")]
        [HttpPost("add-or-update-house-hold-member")]
        public async Task<IActionResult> AddOrUpdateHouseHoldMember([FromBody] HouseHoldMemberDTO dto)
        {
            var result = await houseHoldMemberService.AddOrUpdateAsync(dto);
            if (result == null)
                return BadRequest("Add or Update House Hold Member is failure !");
            return Ok(result);
        }

        [Authorize(Policy = "AdminOrManager")]
        [HttpDelete("delete-house-hold-member")]
        public async Task<IActionResult> DeleteHouseHoldMember(Guid id)
        {
            var result = await houseHoldMemberService.DeleteAsync(id);
            if (result == Guid.Empty)
                return BadRequest("Delete is failure");
            return Ok($"Deleted id:{id}");
        }
    }
}

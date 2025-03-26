using EzConDo_Service.DTO;
using EzConDo_Service.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

        [HttpPost("add-or-update-house-hold-member")]
        public async Task<IActionResult> AddOrUpdateHouseHoldMember([FromBody] HouseHoldMemberDTO dto)
        {
            var result = await houseHoldMemberService.AddOrUpdateAsync(dto);
            if (result == null)
                return BadRequest("Add or Update House Hold Member is failure !");
            return Ok(result);
        }

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

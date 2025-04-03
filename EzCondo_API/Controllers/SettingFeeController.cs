using EzConDo_Service.DTO;
using EzConDo_Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EzCondo_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingFeeController : ControllerBase
    {
        private readonly IPrice_electric_service electric_Service;
        private readonly IPriceWaterTierService priceWaterTierService;
        private readonly IPriceParkingLotService priceParkingLotService;

        public SettingFeeController(IPrice_electric_service electric_Service, IPriceWaterTierService priceWaterTierService, IPriceParkingLotService priceParkingLotService)
        {
            this.electric_Service = electric_Service;
            this.priceWaterTierService = priceWaterTierService;
            this.priceParkingLotService = priceParkingLotService;
        }
        [HttpGet("get-electric-price")]
        public async Task<IActionResult> GetElectricPrice()
        {
            var result = await electric_Service.GetElectricPriceAsync();
            return Ok(result);
        }

        [HttpGet("get-water-price")]
        public async Task<IActionResult> GetWaterPrice()
        {
            var result = await priceWaterTierService.GetWaterPriceAsync();
            if (result == null)
                return Ok("Empty!");
            return Ok(result);
        }

        [HttpGet("get-parking-price")]
        public async Task<IActionResult> GetParkingPrice()
        {
            var result = await priceParkingLotService.GetParkingPriceAsync();
            return Ok(result);
        }

        [Authorize(Policy = "Admin")]
        [HttpPost("add-or-update-electric-price")]
        public async Task<IActionResult> AddElectricPrice([FromBody] PriceElectricTierDTO dto)
        {
            var result = await electric_Service.AddOrUpdateAsync(dto);
            return Ok(result);
        }

        [Authorize(Policy = "Admin")]
        [HttpPost("add-water-price")]
        public async Task<IActionResult> AddWaterPrice([FromBody] PriceWaterTierDTO dto)
        {
            var result = await priceWaterTierService.AddAsync(dto);
            return Ok(result);
        }

        [Authorize(Policy = "Admin")]
        [HttpPatch("update-water-price")]
        public async Task<IActionResult> UpdateWaterPrice([FromBody] PriceWaterTierDTO dto)
        {
            var result = await priceWaterTierService.UpdateAsync(dto);
            return Ok(result);
        }

        [Authorize(Policy = "Admin")]
        [HttpPost("add-parking-price")]
        public async Task<IActionResult> AddPrice([FromBody] PriceParkingLotDTO dto)
        {
            var result = await priceParkingLotService.AddAsync(dto);
            return Ok(result);
        }

        [Authorize(Policy = "Admin")]
        [HttpPatch("update-parking-price")]
        public async Task<IActionResult> UpdateparkingPrice([FromBody] PriceParkingLotDTO dto)
        {
            var result = await priceParkingLotService.UpdateAsync(dto);
            return Ok(result);
        }

        [Authorize(Policy = "Admin")]
        [HttpDelete("delete-electric-price")]
        public async Task<IActionResult> DeleteElectricPrice([FromQuery] Guid electricId)
        {
            var result = await electric_Service.DeleteElectricPriceAsync(electricId);
            return Ok(result);
        }
    }
}

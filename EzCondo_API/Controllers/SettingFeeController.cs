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
        public async Task<IActionResult> AddOrUpdateElectricPrice([FromBody] PriceElectricTierDTO dto)
        {
            var result = await electric_Service.AddOrUpdateAsync(dto);
            return Ok(result);
        }

        [Authorize(Policy = "Admin")]
        [HttpPost("add-or-update-water-price")]
        public async Task<IActionResult> AddOrUpdateWaterPrice([FromBody] PriceWaterTierDTO dto)
        {
            var result = await priceWaterTierService.AddOrUpdateAsync(dto);
            return Ok(result);
        }

        [Authorize(Policy = "Admin")]
        [HttpPost("add-or-update-parking-price")]
        public async Task<IActionResult> AddOrUpdateparkingPrice([FromBody] PriceParkingLotDTO dto)
        {
            var result = await priceParkingLotService.AddOrUpdateAsync(dto);
            return Ok(result);
        }
    }
}

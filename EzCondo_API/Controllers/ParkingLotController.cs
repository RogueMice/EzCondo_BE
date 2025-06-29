﻿using EzConDo_Service.DTO;
using EzConDo_Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EzCondo_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParkingLotController : ControllerBase
    {
        private readonly IParkingLotService parkingLotService;

        public ParkingLotController(IParkingLotService parkingLotService)
        {
            this.parkingLotService = parkingLotService;
        }

        [Authorize(Policy = "AdminOrManager")]
        [HttpGet("parking-dashboard")]
        public async Task<IActionResult> GetParkingDashboard()
        {
            var result = await parkingLotService.GetParkingDashboardAsync();
            return Ok(result);
        }

        [Authorize(Policy = "Manager")]
        [HttpGet("Get-All-Parking-Lot")]
        public async Task<IActionResult> GetAllParkingLots()
        {
            var result = await parkingLotService.GetAllParkingLotsAsync();
            return Ok(result);
        }

        [Authorize(Policy = "Manager")]
        [HttpGet("Get-All-Parking-Lot-Request")]
        public async Task<IActionResult> GetAllParkingLotRequest()
        {
            var result = await parkingLotService.GetAllParkingLotRequestAsync();
            return Ok(result);
        }

        [Authorize(Policy = "Manager")]
        [HttpGet("Get-All-Parking")]
        public async Task<IActionResult> GetAllParking([FromQuery] bool? status, int? day, int? month)
        {
            var result = await parkingLotService.GetAllParkingAsync(status,day,month);
            return Ok(result);
        }

        [Authorize(Policy = "Resident")]
        [HttpGet("Get-My-Parking-Lot")]
        public async Task<IActionResult> GetMyParkingLots()
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new Exception("Token invalid");
            Guid.TryParse(userId, out var user_Id);
            var result = await parkingLotService.GetMyParkingAsync(user_Id);
            return Ok(result);
        }

        [Authorize(Policy = "ManagerOrResident")]
        [HttpGet("Get-Parking-Lot-Detail")]
        public async Task<IActionResult> GetParkingLotDetail([FromQuery] Guid parkingLotId)
        {
            var result = await parkingLotService.GetParkingDetailAsync(parkingLotId);
            return Ok(result);
        }

        [Authorize(Policy = "Resident")]
        [HttpPost("Create-Parking-Lot")]
        public async Task<IActionResult> CreateParkingLots([FromBody] ParkingCardRequestDTO dtos)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new Exception("Token invalid");
            Guid.TryParse(userId, out var user_Id);
            var result = await parkingLotService.AddParkingLotAsync(dtos, user_Id);
            return Ok(result);
        }

        [Authorize(Policy = "Manager")]
        [HttpPost("Accept-Or-Reject-Parking-Lot")]
        public async Task<IActionResult> UpdateOrDeleteParkingLot([FromBody] ParkingLotAcceptOrRejectDTO dto)
        {
            var result = await parkingLotService.UpdateOrDeleteAsync(dto);
            return Ok(result);
        }

        [Authorize(Policy = "Manager")]
        [HttpPatch("Update-Parking-Lot-Detail")]
        public async Task<IActionResult> UpdateParkingLotDetail([FromBody] UpdateParkingLotDetailDTO dto)
        {
            var result = await parkingLotService.UpdateParkingLotDetailAsync(dto);
            return Ok(result);
        }

        [Authorize(Policy = "Manager")]
        [HttpDelete("Delete-Parking-Lot-Detail")]
        public async Task<IActionResult> UpdateParkingLotDetail([FromQuery] Guid parkingLotDetailId)
        {
            var result = await parkingLotService.DeleteParkingLotDetailAsync(parkingLotDetailId);
            return Ok(result);
        }

    }
}

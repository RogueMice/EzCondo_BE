﻿using EzConDo_Service.DTO;
using EzConDo_Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Service;

namespace EzCondo_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ServicesController : ControllerBase
    {
        private readonly IService_service _service;
        private readonly IService_ImageService service_Image;

        public ServicesController(IService_service service, IService_ImageService service_Image)
        {
            _service = service;
            this.service_Image = service_Image;
        }

        [HttpGet("get-all-services")]
        public async Task<IActionResult> GetAllService([FromQuery] string? serviceName, [FromQuery] bool? status)
        {
            var services = await _service.GetAllServicesAsync(serviceName,status);
            if (services == null)
                return BadRequest("Don't have any service !");
            return Ok(services);
        }

        [HttpGet("get-service-by-id")]
        public async Task<IActionResult> GetServiceById([FromQuery] Guid serviceId)
        {
            var services = await _service.GetServiceByIdAsync(serviceId);
            if (services == null)
                return BadRequest("Don't have any service !");
            return Ok(services);
        }

        [HttpGet("get-service-images")]
        public async Task<IActionResult> GetServiceImages(Guid serviceId)
        {
            var serviceImages = await service_Image.GetServiceImagesAsync(serviceId);
            if (serviceImages == null)
                return BadRequest("Don't have any image !");
            return Ok(serviceImages);
        }

        [Authorize(Policy = "Admin")]
        [HttpPost("add-or-update-service")]
        public async Task<IActionResult> AddService([FromBody] AddServiceDTO serviceDTO)
        {
            var service = await _service.AddOrUpdateServiceAsync(serviceDTO);
            if (service == Guid.Empty)
                return BadRequest("Add service is failure !");
            return Ok(service);
        }

        [Authorize(Policy = "Admin")]
        [HttpPost("add-or-update-service-images")]
        public async Task<IActionResult> AddOrUpdateServiceImages([FromForm] Service_ImageDTO serviceImageDTO)
        {
            await service_Image.AddOrUpdateServiceImagesAsync(serviceImageDTO);
            return Ok("AddOrUpdate service images is successful !");
        }
    }
}

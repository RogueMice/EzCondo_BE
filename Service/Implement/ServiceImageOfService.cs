using EzCondo_Data.Context;
using EzCondo_Data.Domain;
using EzConDo_Service.CloudinaryIntegration;
using EzConDo_Service.DTO;
using EzConDo_Service.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.Implement
{
    public class ServiceImageOfService : IService_ImageService
    {
        private readonly ApartmentDbContext dbContext;
        private readonly CloudinaryService cloudinaryService;

        public ServiceImageOfService(ApartmentDbContext dbContext, CloudinaryService cloudinaryService)
        {
            this.dbContext = dbContext;
            this.cloudinaryService = cloudinaryService;
        }
        public async Task AddServiceImagesAsync(Service_ImageDTO serviceImageDTO)
        {
            if (serviceImageDTO.ServiceImages == null || serviceImageDTO.ServiceImages.Count == 0)
                throw new Exception("At least one image is required.");

            bool serviceExists = await dbContext.Services.AnyAsync(s => s.Id == serviceImageDTO.service_Id);
            if (!serviceExists)
                throw new Exception("Service not found");
            var uploadTasks = serviceImageDTO.ServiceImages.Select(async image =>
            {
                try
                {
                    var uploadResult = await cloudinaryService.UploadImageAsync(image);
                    return new ServiceImage
                    {
                        Id = Guid.NewGuid(),
                        ServiceId = serviceImageDTO.service_Id,
                        ImgPath = uploadResult
                    };
                }
                catch (Exception ex)
                {
                    throw new Exception("Upload error !");
                }
            });

            var imageList = (await Task.WhenAll(uploadTasks)).Where(img => img != null).ToList();

            if (imageList.Any())
            {
                await dbContext.ServiceImages.AddRangeAsync(imageList);
                await dbContext.SaveChangesAsync();
            }
            return;
        }
    }
}

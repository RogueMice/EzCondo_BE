using EzCondo_Data.Context;
using EzCondo_Data.Domain;
using EzConDo_Service.CloudinaryIntegration;
using EzConDo_Service.DTO;
using EzConDo_Service.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EzConDo_Service.ExceptionsConfig.CustomException;

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
        public async Task<string?> AddOrUpdateServiceImagesAsync(Service_ImageDTO serviceImageDTO)
        {
            bool serviceExists = await dbContext.Services.AnyAsync(s => s.Id == serviceImageDTO.Service_Id);
            if (!serviceExists)
                throw new NotFoundException("Service not found");

            var currentImages = await dbContext.ServiceImages.Where(s => s.ServiceId == serviceImageDTO.Service_Id).ToListAsync();
            if(currentImages.Any())
            {
                //delete images in the cloud => Nếu đã có ảnh thì xóa ảnh cũ trên cloud
                var deleteTask = currentImages.Select(async image =>
                {
                    try
                    {
                        await cloudinaryService.DeleteImageAsync(image.ImgPath);
                    }
                    catch (Exception ex)
                    {
                        throw new ConflictException("Error deleting image from cloud.");
                    }
                });

                //delete images in database => Xóa ảnh cũ trong database
                dbContext.ServiceImages.RemoveRange(currentImages);
            }

            //upload new images 
            var uploadTasks = serviceImageDTO.ServiceImages.Select(async image =>
            {
                try
                {
                    var uploadResult = await cloudinaryService.UploadImageAsync(image);
                    return new ServiceImage
                    {
                        Id = Guid.NewGuid(),
                        ServiceId = serviceImageDTO.Service_Id,
                        ImgPath = uploadResult
                    };
                }
                catch (Exception ex)
                {
                    throw new ConflictException("Upload error !");
                }
            });

            var imageList = (await Task.WhenAll(uploadTasks)).Where(img => img != null).ToList();

            if (imageList.Any())
            {
                await dbContext.ServiceImages.AddRangeAsync(imageList);
            }

            await dbContext.SaveChangesAsync();
            return "Upload image is successful !";
        }

        public async Task<List<ServiceImageViewDTO>> GetServiceImagesAsync(Guid serviceId)
        {
            var serviceImages = await dbContext.ServiceImages
                .Where(s => s.ServiceId == serviceId)
                .Select(s => new ServiceImageViewDTO
                {
                    Id = s.Id,
                    ServiceId = s.ServiceId,
                    ImgPath = s.ImgPath
                })
                .AsNoTracking()
                .ToListAsync();

            return serviceImages;
        }
    }
}

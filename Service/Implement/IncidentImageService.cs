using EzCondo_Data.Context;
using EzCondo_Data.Domain;
using EzConDo_Service.CloudinaryIntegration;
using EzConDo_Service.DTO;
using EzConDo_Service.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EzConDo_Service.ExceptionsConfig.CustomException;

namespace EzConDo_Service.Implement
{
    public class IncidentImageService : I_IncidentImage
    {
        private readonly ApartmentDbContext dbContext;
        private readonly CloudinaryService cloudinaryService;

        public IncidentImageService(ApartmentDbContext dbContext, CloudinaryService cloudinaryService)
        {
            this.dbContext = dbContext;
            this.cloudinaryService = cloudinaryService;
        }

        public async Task<string?> AddOrUpdateIncidentImageAsync(IncidentImageDTO incidentImageDTO)
        {
            bool incidentExists = await dbContext.Incidents.AnyAsync(i => i.Id == incidentImageDTO.IncidentId);
            if (!incidentExists)
                throw new NotFoundException("Incident not found");

            var currentImages = await dbContext.IncidentImages.Where(i => i.IncidentId == incidentImageDTO.IncidentId).ToListAsync();
            if (currentImages.Any())
            {
                //delete images in the cloud => Nếu đã có ảnh thì xóa ảnh cũ trên cloud
                var deleteTask = currentImages.Select(async image =>
                {
                    try
                    {
                        await cloudinaryService.DeleteImageAsync(image.FilePath);
                    }
                    catch (Exception ex)
                    {
                        throw new ConflictException("Error deleting image from cloud.");
                    }
                });

                //delete images in database => Xóa ảnh cũ trong database
                dbContext.IncidentImages.RemoveRange(currentImages);
            }
            //upload new images 
            var uploadTasks = incidentImageDTO.Images.Select(async image =>
            {
                try
                {
                    var uploadResult = await cloudinaryService.UploadImageAsync(image);
                    return new IncidentImage
                    {
                        Id = Guid.NewGuid(),
                        IncidentId = incidentImageDTO.IncidentId,
                        FilePath = uploadResult
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
                await dbContext.IncidentImages.AddRangeAsync(imageList);
            }

            await dbContext.SaveChangesAsync();
            return "Upload image is successful !";
        }

        public async Task<List<IncidentImageViewDTO>> GetIncidentImageByIdAsync(Guid incidentId)
        {
            var incidentImage = await dbContext.IncidentImages
                .Where(i => i.IncidentId == incidentId)
                .Select(i => new IncidentImageViewDTO
                {
                    Id = i.Id,
                    IncidentId = i.IncidentId,
                    ImgPath = i.FilePath
                }).AsNoTracking().ToListAsync();

            return incidentImage;
        }
    }
}

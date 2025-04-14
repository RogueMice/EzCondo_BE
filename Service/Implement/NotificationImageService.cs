using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EzCondo_Data.Context;
using EzCondo_Data.Domain;
using EzConDo_Service.CloudinaryIntegration;
using EzConDo_Service.DTO;
using EzConDo_Service.Interface;
using Microsoft.EntityFrameworkCore;
using static EzConDo_Service.ExceptionsConfig.CustomException;

namespace EzConDo_Service.Implement
{
    public class NotificationImageService : INotificationImageService
    {
        private readonly ApartmentDbContext dbContext;
        private readonly CloudinaryService cloudinaryService;

        public NotificationImageService(ApartmentDbContext dbContext, CloudinaryService cloudinaryService)
        {
            this.dbContext = dbContext;
            this.cloudinaryService = cloudinaryService;
        }
        public async Task<string?> CreateNotificationImageAsync(NotificationImageDTO dto)
        {
            bool notificationImage = await dbContext.Notifications.AnyAsync(n => n.Id == dto.NotificationId);
            if (!notificationImage)
                throw new NotFoundException("notificationId not found");
            var uploadTask = dto.Image.Select(async image =>
            {
                try
                {
                    var uploadResult = await cloudinaryService.UploadImageAsync(image);
                    return new NotificationImage
                    {
                        Id = Guid.NewGuid(),
                        NotificationId = dto.NotificationId,
                        ImagePath = uploadResult
                    };
                }
                catch (Exception ex)
                {
                    throw new ConflictException("Upload error !");
                }
            });

            var imageList = (await Task.WhenAll(uploadTask)).Where(img => img != null).ToList();

            if (imageList.Any())
            {
                await dbContext.NotificationImages.AddRangeAsync(imageList);
            }

            await dbContext.SaveChangesAsync();
            return "Upload image is successful !";
        }

        public async Task<List<NotificationImageViewDTO>> GetNotificationImageAsync(Guid notificationId)
        {
            var notification = await dbContext.Notifications.FirstOrDefaultAsync(n => n.Id == notificationId) ?? throw new NotFoundException("Notification not found");

            var notificationImages = await dbContext.NotificationImages
                .Where(n => n.NotificationId == notificationId)
                .Select(n => new NotificationImageViewDTO
                {
                    Id = n.Id,
                    Image = n.ImagePath,
                    NotificationId = n.NotificationId
                })
                .ToListAsync();
            return notificationImages;
        }
    }
}

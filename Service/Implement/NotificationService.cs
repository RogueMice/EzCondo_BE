using EzCondo_Data.Context;
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
    public class NotificationService : INotificationService
    {
        private readonly ApartmentDbContext dbContext;

        public NotificationService(ApartmentDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task<string?> CreateNotificationAsync(CreateNotificationDTO notificationDTO, Guid userId)
        {
            var notificationType = notificationDTO.Type.ToLower();

            // Nếu type = "all" thì cho phép, vì đây là trường hợp đặc biệt
            if (notificationType != "all")
            {
                // Kiểm tra trong DB có role này không
                var roleExists = await dbContext.Roles
                    .AnyAsync(r => r.Name.ToLower() == notificationType);

                if (!roleExists)
                {
                    throw new NotFoundException($"Role '{notificationDTO.Type}' không tồn tại trong database.");
                }
            }

            //Create Notification
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                Title = notificationDTO.Title,
                Content = notificationDTO.Content,
                Type = notificationDTO.Type,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };
            dbContext.Notifications.Add(notification);
            await dbContext.SaveChangesAsync();

            // 2) Lấy danh sách user cần nhận thông báo
            List<User> users;
            if (notificationType == "all")
            {
                users = await dbContext.Users.ToListAsync();
            }
            else
            {
                users = await dbContext.Users
                    .Where(u => u.Role.Name.ToLower() == notificationType)
                    .ToListAsync();
            }

            // 3) Tạo các bản ghi NotificationReceivers
            var receivers = users.Select(u => new NotificationReceiver
            {
                Id = Guid.NewGuid(),
                NotificationId = notification.Id,
                UserId = u.Id,
                Role = notificationType,         
                IsRead = false,
                ReadAt = null
            }).ToList();

            dbContext.NotificationReceivers.AddRange(receivers);
            await dbContext.SaveChangesAsync();

            return "Add notification successfull !";
        }
    }
}

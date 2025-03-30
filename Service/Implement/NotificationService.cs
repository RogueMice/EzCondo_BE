using EzCondo_Data.Context;
using EzCondo_Data.Domain;
using EzConDo_Service.DTO;
using EzConDo_Service.FirebaseIntegration;
using EzConDo_Service.Interface;
using EzConDo_Service.SignalR_Integration;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static EzConDo_Service.ExceptionsConfig.CustomException;

namespace EzConDo_Service.Implement
{
    public class NotificationService : INotificationService
    {
        private readonly ApartmentDbContext dbContext;
        private readonly IFirebasePushNotificationService firebasePush;
        private readonly IHubContext<NotificationHub> hubContext;

        public NotificationService(ApartmentDbContext dbContext, IFirebasePushNotificationService firebasePush, IHubContext<NotificationHub> hubContext)
        {
            this.dbContext = dbContext;
            this.firebasePush = firebasePush;
            this.hubContext = hubContext;
        }

        public async Task<Guid?> CreateNotificationAsync(CreateNotificationDTO notificationDTO, Guid userId)
        {
            var notificationType = notificationDTO.Type.ToLower();

            // Validate role
            if (notificationType != "all")
            {
                var roleExists = await dbContext.Roles
                    .AnyAsync(r => r.Name.ToLower() == notificationType)
                    .ConfigureAwait(false);

                if (!roleExists)
                {
                    throw new NotFoundException($"Role '{notificationDTO.Type}' không tồn tại trong database.");
                }
            }

            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                Title = notificationDTO.Title,
                Content = notificationDTO.Content,
                Type = notificationDTO.Type,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            var userIds = notificationType == "all"
                ? await dbContext.Users.Select(u => u.Id).ToListAsync().ConfigureAwait(false)
                : await dbContext.Users
                    .Where(u => u.Role.Name.ToLower() == notificationType)
                    .Select(u => u.Id)
                    .ToListAsync()
                    .ConfigureAwait(false);

            var receivers = userIds.Select(userId => new NotificationReceiver
            {
                Id = Guid.NewGuid(),
                NotificationId = notification.Id,
                UserId = userId,
                Role = notificationType,
                IsRead = false,
                ReadAt = null
            }).ToList();

            await using var transaction = await dbContext.Database.BeginTransactionAsync().ConfigureAwait(false);

            try
            {
                dbContext.Notifications.Add(notification);
                dbContext.NotificationReceivers.AddRange(receivers);

                await dbContext.SaveChangesAsync().ConfigureAwait(false);
                await transaction.CommitAsync().ConfigureAwait(false);
            }
            catch
            {
                await transaction.RollbackAsync().ConfigureAwait(false);
                throw;
            }

            // 1) Lấy danh sách fcm_token từ user_device
            var deviceTokens = await dbContext.UserDevices
                .Where(ud => userIds.Contains(ud.UserId) && ud.IsActive)
                .Select(ud => ud.FcmToken)
                .ToListAsync();

            // 2) gửi thông báo đẩy
            if (deviceTokens.Any())
            {
                await firebasePush.SendPushNotificationAsync(
                    notification.Title,
                    notification.Content,
                    deviceTokens
                );
            }

            return notification.Id;
        }

        public async Task<NotificationListDTO> GetNotificationsAsync(bool isRead, int page, int pageSize, Guid userId)
        {
            var query = dbContext.NotificationReceivers
                        .Include(nr => nr.Notification)
                        .Where(nr => nr.UserId == userId && nr.IsRead == isRead) //Check is read
                        .OrderByDescending(nr => nr.Notification.CreatedAt);

            var totalCount = await query.CountAsync();
            var notifications = await query
                                .Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .Select(nr => new NotificationDTO
                                {
                                    Id = nr.Notification.Id,
                                    Title = nr.Notification.Title,
                                    Content = nr.Notification.Content,
                                    Type = nr.Notification.Type,
                                    CreatedAt = nr.Notification.CreatedAt,
                                    IsRead = nr.IsRead.Value,
                                    ReadAt = nr.ReadAt,
                                    LastModified = nr.Notification.CreatedAt
                                })
                                .ToListAsync();
            return new NotificationListDTO
            {
                Total = totalCount,
                Notifications = notifications
            };
        }

        public async Task<NotificationViewListDTO> AdminGetNotificationsAsync(int page, int pageSize, int? day)
        {
            var query = dbContext.NotificationReceivers
                                    .Include(nr => nr.Notification)
                                    .AsQueryable();
            // Filter by days
            if (day.HasValue && day > 0)
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-day.Value);
                query = query.Where(nr => nr.Notification.CreatedAt >= cutoffDate);
            }

            // Ordering
            query = query.OrderByDescending(nr => nr.Notification.CreatedAt);

            // Total count AFTER filtering
            var totalCount = await query.CountAsync();

            // Pagination
            var notifications = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(nr => new NotificationViewDTO
                {
                    Id = nr.Notification.Id,
                    Title = nr.Notification.Title,
                    Content = nr.Notification.Content,
                    Type = nr.Notification.Type,
                    CreatedAt = nr.Notification.CreatedAt
                })
                .ToListAsync();

            return new NotificationViewListDTO
            {
                Total = totalCount,
                Notifications = notifications
            };
        }

        public async Task<string?> MarkAsReadAsync(MarkAsReadRequestDTO request, Guid userId)
        {
            var notificationsToMark = await dbContext.NotificationReceivers
                .Where(nr => nr.UserId == userId &&
                             request.NotificationIds.Contains(nr.NotificationId) &&
                             nr.IsRead == false)
                .ToListAsync();

            if (!notificationsToMark.Any())
                throw new NotFoundException("Không tìm thấy thông báo hợp lệ");

            foreach (var notification in notificationsToMark)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
            }

            await dbContext.SaveChangesAsync();

            // Gửi realtime update cho user
            await hubContext.Clients
                .Group(userId.ToString())
                .SendAsync("NotificationRead", new
                {
                    NotificationIds = request.NotificationIds,
                    ReadAt = DateTime.UtcNow
                });
            return "Mark as read successfull!";
        }
    }
}

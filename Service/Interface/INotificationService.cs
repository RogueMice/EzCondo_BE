using EzCondo_Data.Context;
using EzConDo_Service.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.Interface
{
    public interface INotificationService
    {
        Task<Guid?> CreateNotificationAsync(CreateNotificationDTO notificationDTO, Guid userId);

        Task<List<Guid>> CreateNotificationsToUsersAsync(List<SendNotificationToUserDTO> notificationDTOs, Guid userId);

        Task<NotificationListDTO> GetNotificationsAsync(bool? isRead, int page, int pageSize, Guid userId, string? type);

        Task<NotificationViewListDTO> AdminGetNotificationsAsync(int page, int pageSize, int? day, string? receiver, string? type);

        Task<string?> MarkAsReadAsync(MarkAsReadRequestDTO request, Guid userId);

        Task<string> UserCreateNotificationAsync(CreateNotificationDTO notificationDTO, Guid userId);
    }
}

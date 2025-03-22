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
        Task<string?> CreateNotificationAsync(CreateNotificationDTO notificationDTO, Guid userId);

        Task<NotificationListDTO> GetNotificationsAsync(bool isRead, int page, int pageSize, Guid userId);

        Task<NotificationViewListDTO> AdminGetNotificationsAsync(int page, int pageSize, int? day);

        Task<string?> MarkAsReadAsync(MarkAsReadRequestDTO request, Guid userId);
    }
}

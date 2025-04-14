using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EzConDo_Service.DTO;

namespace EzConDo_Service.Interface
{
    public interface INotificationImageService
    {
        Task<string?> CreateNotificationImageAsync(NotificationImageDTO dto);

        Task<List<NotificationImageViewDTO>> GetNotificationImageAsync(Guid notificationId);
    }
}

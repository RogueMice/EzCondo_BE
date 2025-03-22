using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.FirebaseIntegration
{
    public interface IFirebasePushNotificationService
    {
        Task SendPushNotificationAsync(string title, string body, List<string> deviceTokens);
    }
}

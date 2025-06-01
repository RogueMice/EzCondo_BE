using Azure;
using FirebaseAdmin.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.FirebaseIntegration
{
    public class FirebasePushNotificationService : IFirebasePushNotificationService
    {
        public async Task SendPushNotificationAsync(string title, string body, List<string> deviceTokens)
        {
            if (deviceTokens == null || deviceTokens.Count == 0) return;

            var message = new MulticastMessage
            {
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Tokens = deviceTokens
            };

            try
            {
                var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);
                Console.WriteLine($"[FCM] Tổng số token gửi: {message.Tokens.Count}");
                Console.WriteLine($"[FCM] Gửi thành công: {response.SuccessCount} | Thất bại: {response.FailureCount}");

                for (int i = 0; i < response.Responses.Count; i++)
                {
                    var token = message.Tokens[i];
                    var res = response.Responses[i];
                    if (res.IsSuccess)
                    {
                        Console.WriteLine($"[FCM] Token {token} nhận thông báo thành công.");
                    }
                    else
                    {
                        Console.WriteLine($"[FCM] Token {token} gửi thất bại: {res.Exception?.Message}");
                    }
                }
            }
            catch (FirebaseMessagingException ex)
            {
                Console.WriteLine($"[FCM] Error: {ex.MessagingErrorCode}");
                Console.WriteLine($"[FCM] Exception: {ex}");
                Console.WriteLine($"[FCM] InnerException: {ex.InnerException}");
            }
        }
    }
}

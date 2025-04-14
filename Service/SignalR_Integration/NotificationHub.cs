using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
namespace EzConDo_Service.SignalR_Integration
{
    public class NotificationHub: Hub
    {
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }
        public override async Task OnConnectedAsync()
        {
            var userRole = Context.User?.FindFirst(ClaimTypes.Role)?.Value?.ToLower();
            if (userRole == "manager")
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Managers");
                Console.WriteLine($"Kết nối {Context.ConnectionId} của Manager đã được thêm vào group Managers.");
            }
            await base.OnConnectedAsync();
        }
    }
}

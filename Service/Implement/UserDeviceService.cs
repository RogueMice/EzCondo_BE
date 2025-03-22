using EzCondo_Data.Context;
using EzConDo_Service.DTO;
using EzConDo_Service.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.Implement
{
    public class UserDeviceService: IUserDeviceService
    {
        private readonly ApartmentDbContext dbContext;

        public UserDeviceService(ApartmentDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task<string?> UpdateFcmToken(UpdateFcmTokenDTO dto)
        {
            var userDevice = await dbContext.UserDevices.FirstOrDefaultAsync(ud => ud.UserId == dto.UserId && ud.Type == dto.Type);

            if (userDevice == null)
            {
                userDevice = new UserDevice
                {
                    Id = Guid.NewGuid(),
                    Type = dto.Type,
                    FcmToken = dto.FcmToken,
                    IsActive = true,
                    UserId = (Guid)dto.UserId
                };
                dbContext.UserDevices.Add(userDevice);
            }
            else
            {
                userDevice.FcmToken = dto.FcmToken;
                userDevice.IsActive = true;
            }

            await dbContext.SaveChangesAsync();
            return "FCM token updated.";
        }
    }
}

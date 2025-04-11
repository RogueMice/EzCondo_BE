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
        public async Task<string?> AddOrUpdateFcmToken(UpdateFcmTokenDTO dto)
        {
            var userDevice = await dbContext.UserDevices.FirstOrDefaultAsync(ud => ud.UserId == dto.UserId && ud.Type == dto.Type);

            if (userDevice == null)
            {
                if (dto.UserId == null)
                {
                    throw new ArgumentNullException(nameof(dto.UserId), "UserId cannot be null.");
                }

                userDevice = new UserDevice
                {
                    FcmToken = dto.FcmToken,
                    Type = dto.Type,
                    IsActive = dto.IsActive ?? true,
                    UserId = (Guid)dto.UserId
                };
                dbContext.UserDevices.Add(userDevice);
            }
            else
            {
                if (dto.IsActive == null)
                {
                    throw new ArgumentNullException(nameof(dto.IsActive), "IsActive cannot be null.");
                }

                userDevice.IsActive = (bool)dto.IsActive;
            }

            await dbContext.SaveChangesAsync();
            return "FCM token updated.";
        }
    }
}

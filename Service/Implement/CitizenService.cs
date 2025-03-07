using EzCondo_Data.Context;
using EzCondo_Data.Domain;
using EzConDo_Service.CloudinaryIntegration;
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
    public class CitizenService : ICitizenService
    {
        private readonly ApartmentDbContext dbContext;
        private readonly CloudinaryService cloudinaryService;

        public CitizenService(ApartmentDbContext dbContext, CloudinaryService cloudinaryService )
        {
            this.dbContext = dbContext;
            this.cloudinaryService = cloudinaryService;
        }

        public async Task<Citizen> AddOrUpdateCitizenAsync(CitizenDTO citizenDTO)
        {
            var user = await dbContext.Users.AsNoTracking()
                                  .FirstOrDefaultAsync(u => u.Id == citizenDTO.UserId)
                                  ?? throw new Exception("User không tồn tại !");
            var citizen = await dbContext.Citizens.FirstOrDefaultAsync(c => c.UserId == citizenDTO.UserId);
            if (citizen is not null)
            {
                citizen.No = citizenDTO.No;
                citizen.DateOfIssue = citizenDTO.DateOfIssue;
                citizen.DateOfExpiry = citizenDTO.DateOfExpiry;
            }
            else
            {
                // Upload image on the Cloudinary if have
                Task<string?> frontImageTask = citizenDTO.FrontImage != null
                                                ? cloudinaryService.UploadImageAsync(citizenDTO.FrontImage)
                                                : Task.FromResult<string?>(null);
                Task<string?> backImageTask = citizenDTO.BackImage != null
                                                ? cloudinaryService.UploadImageAsync(citizenDTO.BackImage)
                                                : Task.FromResult<string?>(null);

                await Task.WhenAll(frontImageTask, backImageTask);
                //add citizen
                citizen = new Citizen
                {
                    UserId = citizenDTO.UserId,
                    No = citizenDTO.No,
                    DateOfIssue = citizenDTO.DateOfIssue,
                    DateOfExpiry = citizenDTO.DateOfExpiry,
                    FrontImage = frontImageTask.Result,
                    BackImage = backImageTask.Result
                };
                dbContext.Add(citizen);
            }

            await dbContext.SaveChangesAsync();
            return citizen;
        }
    }
}

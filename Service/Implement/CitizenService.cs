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
                                  .FirstOrDefaultAsync(u => u.Id == citizenDTO.userId)
                                  ?? throw new Exception("User invalid !");
            var citizen = await dbContext.Citizens.FirstOrDefaultAsync(c => c.UserId == citizenDTO.userId);
            if (citizen is not null)
            {
                citizen.No = citizenDTO.no;
                citizen.DateOfIssue = citizenDTO.dateOfIssue;
                citizen.DateOfExpiry = citizenDTO.dateOfExpiry;
            }
            else
            {
                // Upload image on the Cloudinary if have
                Task<string?> frontImageTask = citizenDTO.frontImage != null
                                                ? cloudinaryService.UploadImageAsync(citizenDTO.frontImage)
                                                : Task.FromResult<string?>(null);
                Task<string?> backImageTask = citizenDTO.backImage != null
                                                ? cloudinaryService.UploadImageAsync(citizenDTO.backImage)
                                                : Task.FromResult<string?>(null);
                await Task.WhenAll(frontImageTask, backImageTask);
                citizen = new Citizen
                {
                    UserId = citizenDTO.userId,
                    No = citizenDTO.no,
                    DateOfIssue = citizenDTO.dateOfIssue,
                    DateOfExpiry = citizenDTO.dateOfExpiry,
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

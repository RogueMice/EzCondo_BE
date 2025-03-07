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
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == citizenDTO.UserId) ?? throw new Exception("User không tồn tại !");
            var citizen = await dbContext.Citizens.FirstOrDefaultAsync(x => x.UserId == citizenDTO.UserId);
            if(citizen is not null)
            {
                citizen.No = citizenDTO.No;
                citizen.DateOfIssue = citizenDTO.DateOfIssue;
                citizen.DateOfExpiry = citizenDTO.DateOfExpiry;
            }
            else
            {
                // Upload image on the Cloudinary if have
                string? frontImageUrl = citizenDTO.FrontImage != null ? await cloudinaryService.UploadImageAsync(citizenDTO.FrontImage) : citizen?.FrontImage;
                string? backImageUrl = citizenDTO.BackImage != null ? await cloudinaryService.UploadImageAsync(citizenDTO.BackImage) : citizen?.BackImage;
                //add citizen
                citizen = new Citizen
                {
                    UserId = citizenDTO.UserId,
                    No = citizenDTO.No,
                    DateOfIssue = citizenDTO.DateOfIssue,
                    DateOfExpiry = citizenDTO.DateOfExpiry,
                    FrontImage = frontImageUrl,
                    BackImage = backImageUrl
                };
                dbContext.Add(citizen);
            }

            await dbContext.SaveChangesAsync();
            return citizen;
        }
    }
}

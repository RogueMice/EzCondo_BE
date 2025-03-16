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
using static EzConDo_Service.ExceptionsConfig.CustomException;

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
                                  ?? throw new NotFoundException($"UserId {citizenDTO.UserId} is not found");
            bool noExists = await dbContext.Citizens.AnyAsync(c => c.No == citizenDTO.No && c.UserId != citizenDTO.UserId);
            if (noExists)
            {
                throw new ConflictException($"NO: {citizenDTO.No} is already in use!");
            }

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

        public async Task<List<CitizenViewDTO>> GetAllCitizensAsync()
        {
            var citizens = await dbContext.Citizens
                                        .AsNoTracking()
                                        .Select(c => new CitizenViewDTO
                                        {
                                            UserId = c.UserId,
                                            No = c.No,
                                            DateOfExpiry = (DateOnly)c.DateOfExpiry,
                                            DateOfIssue = c.DateOfIssue,
                                            FrontImage = c.FrontImage,
                                            BackImage = c.BackImage
                                        }).ToListAsync();
            return citizens;
        }
    }
}

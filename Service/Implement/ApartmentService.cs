using EzCondo_Data.Context;
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
    public class ApartmentService : IApartmentService
    {
        private readonly ApartmentDbContext dbContext;

        public ApartmentService(ApartmentDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<ApartmentViewDTO> GetMyApartmentAsync(Guid userId)
        {
            var apartment = await dbContext.Apartments.FirstOrDefaultAsync(x => x.UserId == userId) ?? throw new NotFoundException($"Userid: {userId} is not found !");

            return new ApartmentViewDTO
            {
                Id = apartment.Id,
                ApartmentNumber = apartment.ApartmentNumber,
                ResidentNumber = apartment.ResidentNumber,
                Acreage = apartment.Acreage,
                Description = apartment.Description
            };
        }

        public async Task<List<ApartmentViewDTO>> GetApartmentAsync(Guid? userId, string? apartmentNumber)
        {
            var query = dbContext.Apartments.AsNoTracking().AsQueryable();

            if (userId.HasValue)
            {
                query = query.Where(x => x.UserId == userId.Value);
            }

            if (!string.IsNullOrEmpty(apartmentNumber))
            {
                query = query.Where(x => x.ApartmentNumber.Contains(apartmentNumber));
            }

            var apartments = await query
                .Select(apartment => new ApartmentViewDTO
                {
                    Id = apartment.Id,
                    ApartmentNumber = apartment.ApartmentNumber,
                    ResidentNumber = apartment.ResidentNumber,
                    Acreage = apartment.Acreage,
                    Description = apartment.Description
                }).ToListAsync();

            return apartments;
        }

        public async Task<string?> UpdateApartmentAsync(ApartmentUpdateDTO apartmentDto)
        {
            var apartment = await dbContext.Apartments.FirstOrDefaultAsync(x => x.Id == apartmentDto.Id) ?? throw new NotFoundException($"ApartmentId: {apartmentDto.Id} is not found !");
            apartment.Acreage = apartmentDto.Acreage;
            apartment.Description = apartmentDto.Description;
            await dbContext.SaveChangesAsync();
            return "Update apartment successfully !";
        }
    }
}

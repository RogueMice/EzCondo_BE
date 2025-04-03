using EzCondo_Data.Context;
using EzCondo_Data.Domain;
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
                Description = apartment.Description,
                UserId = apartment.UserId
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
                    Description = apartment.Description,
                    UserId = apartment.UserId
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

        public async Task<Guid?> AddApartmentAsync(ApartmentViewDTO apartmentDto)
        {
            var existingApartment = await dbContext.Apartments.FirstOrDefaultAsync(x => x.ApartmentNumber == apartmentDto.ApartmentNumber);

            if (existingApartment != null)
            {
                throw new BadRequestException("Apartment number already exist !");
            }

            var apartment = new Apartment
            {
                Id = Guid.NewGuid(),
                ApartmentNumber = apartmentDto.ApartmentNumber,
                ResidentNumber = 0,
                Acreage = apartmentDto.Acreage,
                Description = apartmentDto.Description,
                UserId = null
            };
            await dbContext.Apartments.AddAsync(apartment);
            dbContext.SaveChanges();
            return apartment.Id;
        }

        public async Task<ApartmentViewDTO> GetApartmentByIdAsync(Guid apartmentId)
        {
            var apartment = await dbContext.Apartments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == apartmentId) ?? throw new NotFoundException($"ApartmentId: {apartmentId} is not found !");
            return new ApartmentViewDTO
            {
                Id = apartment.Id,
                ApartmentNumber = apartment.ApartmentNumber,
                ResidentNumber = apartment.ResidentNumber,
                Acreage = apartment.Acreage,
                Description = apartment.Description,
                UserId = apartment.UserId
            };
        }
    }
}

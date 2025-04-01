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
    public class PriceParkingLotService : IPriceParkingLotService
    {
        private readonly ApartmentDbContext dbContext;

        public PriceParkingLotService(ApartmentDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task<Guid?> AddAsync(PriceParkingLotDTO dto)
        {
            var existtingParkingLot = dbContext.PriceParkingLots.FirstOrDefault();
            if (existtingParkingLot != null)
            {
                throw new ConflictException("Price parking lot already exists");
            }

            var parkingLot = new PriceParkingLot
            {
                Id = Guid.NewGuid(),
                PricePerMotor = dto.PricePerMotor,
                PricePerOto = dto.PricePerOto
            };
            dbContext.PriceParkingLots.Add(parkingLot);
            await dbContext.SaveChangesAsync();
            return parkingLot.Id;
        }

        public async Task<Guid?> UpdateAsync(PriceParkingLotDTO dto)
        {
            var parkingLot = dbContext.PriceParkingLots.FirstOrDefault(x => x.Id == dto.Id) ?? throw new Exception("Price parking lot not found");
            parkingLot.PricePerMotor = dto.PricePerMotor;
            parkingLot.PricePerOto = dto.PricePerOto;
            dbContext.PriceParkingLots.Update(parkingLot);
            await dbContext.SaveChangesAsync();
            return parkingLot.Id;
        }

        public async Task<PriceParkingLotDTO> GetParkingPriceAsync()
        {
            var parkingLots = await dbContext.PriceParkingLots.AsNoTracking().FirstOrDefaultAsync();
            if (parkingLots == null)
            {
                return new PriceParkingLotDTO
                {
                    Id = Guid.Empty,
                    PricePerMotor = 0,
                    PricePerOto = 0
                };
            }
            return new PriceParkingLotDTO
            {
                Id = parkingLots.Id,
                PricePerMotor = parkingLots.PricePerMotor,
                PricePerOto = parkingLots.PricePerOto
            };
        }
    }
}

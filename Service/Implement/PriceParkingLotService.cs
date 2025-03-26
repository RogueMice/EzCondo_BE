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

namespace EzConDo_Service.Implement
{
    public class PriceParkingLotService : IPriceParkingLotService
    {
        private readonly ApartmentDbContext dbContext;

        public PriceParkingLotService(ApartmentDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task<Guid?> AddOrUpdateAsync(PriceParkingLotDTO dto)
        {
            var parkingLot = dbContext.PriceParkingLots.FirstOrDefault(x => x.Id == dto.Id) ?? new PriceParkingLot();

            //add
            if (parkingLot.Id == Guid.Empty)
            {
                parkingLot.Id = Guid.NewGuid();
                parkingLot.PricePerMotor = dto.PricePerMotor;
                parkingLot.PricePerOto = dto.PricePerOto;
                dbContext.PriceParkingLots.Add(parkingLot);
            }
            //update
            else
            {
                parkingLot.PricePerMotor = dto.PricePerMotor;
                parkingLot.PricePerOto = dto.PricePerOto;
                dbContext.PriceParkingLots.Update(parkingLot);
            }
            await dbContext.SaveChangesAsync();
            return parkingLot.Id;
        }

        public Task<List<PriceParkingLotDTO>> GetParkingPriceAsync()
        {
            var parkingLots = dbContext.PriceParkingLots.AsNoTracking().Select(x => new PriceParkingLotDTO
            {
                Id = x.Id,
                PricePerMotor = x.PricePerMotor,
                PricePerOto = x.PricePerOto
            }).ToListAsync();
            return parkingLots;
        }
    }
}

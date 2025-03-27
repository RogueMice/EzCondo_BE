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
    public class PriceWaterTierService : IPriceWaterTierService
    {
        private readonly ApartmentDbContext dbContext;

        public PriceWaterTierService(ApartmentDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task<Guid?> AddOrUpdateAsync(PriceWaterTierDTO priceWaterTierDTO)
        {
            var priceWaterTier = dbContext.PriceWaterTiers.FirstOrDefault(x => x.Id == priceWaterTierDTO.Id) ?? new PriceWaterTier();

            //add
            if(priceWaterTier.Id == Guid.Empty)
            {
                priceWaterTier.Id = Guid.NewGuid();
                priceWaterTier.PricePerM3 = priceWaterTierDTO.PricePerM3;
                dbContext.PriceWaterTiers.Add(priceWaterTier);
            }
            //update
            else
            {
                priceWaterTier.PricePerM3 = priceWaterTierDTO.PricePerM3;
                dbContext.PriceWaterTiers.Update(priceWaterTier);
            }
            await dbContext.SaveChangesAsync();
            return priceWaterTier.Id;
        }

        public async Task<PriceWaterTierDTO> GetWaterPriceAsync()
        {
            var priceWaterTier = await dbContext.PriceWaterTiers.AsNoTracking().FirstOrDefaultAsync();
            if (priceWaterTier == null)
            {
                // Handle the case when no record is found
                return null; // or throw an exception, or return a default PriceWaterTierDTO
            }
            return new PriceWaterTierDTO
            {
                Id = priceWaterTier.Id,
                PricePerM3 = priceWaterTier.PricePerM3
            };
        }
    }
}

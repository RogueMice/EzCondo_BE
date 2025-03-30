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
    public class PriceWaterTierService : IPriceWaterTierService
    {
        private readonly ApartmentDbContext dbContext;

        public PriceWaterTierService(ApartmentDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task<Guid?> AddAsync(PriceWaterTierDTO priceWaterTierDTO)
        {
            var existingPriceWaterTier = dbContext.PriceWaterTiers.FirstOrDefault();
            if (existingPriceWaterTier != null)
            {
                throw new ConflictException("priceWater already exists");
            }

            //add
            var priceWaterTier = new PriceWaterTier
            {
                Id = Guid.NewGuid(),
                PricePerM3 = priceWaterTierDTO.PricePerM3
            };

            dbContext.PriceWaterTiers.Add(priceWaterTier);
            await dbContext.SaveChangesAsync();
            return priceWaterTier.Id;
        }

        public async Task<Guid?> UpdateAsync(PriceWaterTierDTO priceWaterTierDTO)
        {
            var priceWaterTier = dbContext.PriceWaterTiers.FirstOrDefault(x => x.Id == priceWaterTierDTO.Id) ?? throw new NotFoundException("priceWater id invalid");
                priceWaterTier.PricePerM3 = priceWaterTierDTO.PricePerM3;
                dbContext.PriceWaterTiers.Update(priceWaterTier);
            await dbContext.SaveChangesAsync();
            return priceWaterTier.Id;
        }

        public async Task<PriceWaterTierDTO> GetWaterPriceAsync()
        {
            var priceWaterTier = await dbContext.PriceWaterTiers.AsNoTracking().FirstOrDefaultAsync();
            if (priceWaterTier == null)
            {
                throw new NotFoundException("Price water tier not found");
            }
            return new PriceWaterTierDTO
            {
                Id = priceWaterTier.Id,
                PricePerM3 = priceWaterTier.PricePerM3
            };
        }
    }
}

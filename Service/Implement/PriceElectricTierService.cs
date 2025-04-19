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
    public class PriceElectricTierService : IPrice_electric_service
    {
        private readonly ApartmentDbContext dbContext;

        public PriceElectricTierService(ApartmentDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task<Guid?> AddOrUpdateAsync(PriceElectricTierDTO priceElectricTierDTO)
        {
            var priceElectricTier = dbContext.PriceElectricTiers.FirstOrDefault(x => x.Id == priceElectricTierDTO.Id) ?? new PriceElectricTier();

            //add
            if(priceElectricTier.Id == Guid.Empty)
            {
                priceElectricTier.Id = Guid.NewGuid();
                priceElectricTier.MinKWh = priceElectricTierDTO.MinKWh;
                priceElectricTier.MaxKWh = priceElectricTierDTO.MaxKWh;
                priceElectricTier.PricePerKWh = priceElectricTierDTO.PricePerKWh;
                dbContext.PriceElectricTiers.Add(priceElectricTier);
            }
            //update
            else
            {
                priceElectricTier.MinKWh = priceElectricTierDTO.MinKWh;
                priceElectricTier.MaxKWh = priceElectricTierDTO.MaxKWh;
                priceElectricTier.PricePerKWh = priceElectricTierDTO.PricePerKWh;
                dbContext.PriceElectricTiers.Update(priceElectricTier);
            }
            await dbContext.SaveChangesAsync();
            return priceElectricTier.Id;
        }

        public Task<List<PriceElectricTierDTO>> GetElectricPriceAsync()
        {
            var priceElectricTiers = dbContext.PriceElectricTiers.AsNoTracking().Select(x => new PriceElectricTierDTO
            {
                Id = x.Id,
                MinKWh = x.MinKWh,
                MaxKWh = x.MaxKWh,
                PricePerKWh = x.PricePerKWh
            }).ToListAsync();
            return priceElectricTiers;
        }

        public async Task<Guid?> DeleteElectricPriceAsync(Guid? electricPriceId)
        {
            var priceElectricTier = await dbContext.PriceElectricTiers.FirstOrDefaultAsync(x => x.Id == electricPriceId) ?? throw new NotFoundException($"Electric price with id {electricPriceId} not found");
            dbContext.PriceElectricTiers.Remove(priceElectricTier);
            await dbContext.SaveChangesAsync();
            return priceElectricTier.Id;
        }
    }
}

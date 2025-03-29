using EzConDo_Service.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.Interface
{
    public interface IPriceWaterTierService
    {
        Task<Guid?> AddAsync(PriceWaterTierDTO priceWaterTierDTO);

        Task<Guid?> UpdateAsync(PriceWaterTierDTO priceWaterTierDTO);

        Task<PriceWaterTierDTO> GetWaterPriceAsync();
    }
}

using EzConDo_Service.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.Interface
{
    public interface IPrice_electric_service
    {
        Task<Guid?> AddOrUpdateAsync(PriceElectricTierDTO priceElectricTierDTO);

        Task<List<PriceElectricTierDTO>> GetElectricPriceAsync();

        Task<Guid?> DeleteElectricPriceAsync(Guid? electricPriceId);
    }
}

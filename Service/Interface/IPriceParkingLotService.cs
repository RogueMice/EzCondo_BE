using EzConDo_Service.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.Interface
{
    public interface IPriceParkingLotService
    {
        Task<Guid?> AddAsync(PriceParkingLotDTO dto);

        Task<List<PriceParkingLotDTO>> GetParkingPriceAsync();

        Task<Guid?> UpdateAsync(PriceParkingLotDTO dto);
    }
}

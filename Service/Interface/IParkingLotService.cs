using EzConDo_Service.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.Interface
{
    public interface IParkingLotService
    {
        Task<List<Guid>> AddParkingLotAsync(ParkingCardRequestDTO dtos, Guid userId);

        Task<List<ParkingLotViewDTO>> GetAllParkingLotsAsync();

        Task<string> UpdateOrDeleteAsync(ParkingLotAcceptOrRejectDTO dto);

        Task<List<ParkingLotDetailViewDTO>> GetParkingDetailAsync(Guid parkingLotId);

        Task<string> UpdateParkingLotDetailAsync(UpdateParkingLotDetailDTO dto);

        Task<Guid> DeleteParkingLotDetailAsync(Guid id);
    }
}

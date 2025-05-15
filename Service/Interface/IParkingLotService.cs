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

        Task<List<ParkingLotViewDTO>> GetAllParkingLotRequestAsync();

        Task<string> UpdateOrDeleteAsync(ParkingLotAcceptOrRejectDTO dto);

        Task<List<ParkingLotDetailViewDTO>> GetParkingDetailAsync(Guid parkingLotId);

        Task<string> UpdateParkingLotDetailAsync(UpdateParkingLotDetailDTO dto);

        Task<Guid> DeleteParkingLotDetailAsync(Guid id);

        Task UpdateOverdueParkingBillsAsync();

        Task CreateRecurringParkingBillAsync(Guid previousInvoiceId);

        Task<List<ParkingViewDTO>> GetAllParkingAsync(bool? status, int? day, int? month);

        Task<List<ParkingLotViewDTO>> GetMyParkingAsync(Guid userId);

        Task GenerateMonthlyBillsAsync();
    }
}

using EzConDo_Service.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.Interface
{
    public interface IOtherService
    {
        Task<Guid?> AddOrUpdateAsync(OtherServiceDTO dto);

        Task<List<OtherServiceDTO>> GetOtherServiceAsync();

        Task<Guid?> DeleteOtherServiceAsync(Guid otherSerivceId);

        Task GenerateMonthlyBillsAsync();
    }
}

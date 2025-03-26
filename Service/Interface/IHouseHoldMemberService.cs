using EzConDo_Service.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.Interface
{
    public interface IHouseHoldMemberService
    {
        Task<Guid?> AddOrUpdateAsync(HouseHoldMemberDTO houseHoldMemberDTO);

        Task<Guid?> DeleteAsync(Guid id);

        Task<MyHouseHoldResponseDTO> GetMyHoldHouseMemberAsync(Guid user_id);
    }
}

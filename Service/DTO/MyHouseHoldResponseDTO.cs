using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.DTO
{
    public class MyHouseHoldResponseDTO
    {
        public UserDTO User { get; set; }

        public List<MyHouseHoldMemberDTO> Members { get; set; }
    }
}

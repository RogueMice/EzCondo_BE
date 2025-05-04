using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.DTO
{
    public class ParkingLotAcceptOrRejectDTO
    {
        public Guid ParkingLotId { get; set; }

        public bool Accept { get; set; }
    }
}

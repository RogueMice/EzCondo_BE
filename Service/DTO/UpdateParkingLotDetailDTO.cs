using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.DTO
{
    public class UpdateParkingLotDetailDTO
    {
        public Guid ParkingLotDetailId { get; set; }

        public bool? Status { get; set; }

        public bool? Checking { get; set; }
    }
}

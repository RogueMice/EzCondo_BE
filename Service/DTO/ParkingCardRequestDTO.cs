using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.DTO
{
    public class ParkingCardRequestDTO
    {
        public Guid? Id { get; set; }

        public int? NumberOfMotorbikes { get; set; }

        public int? NumberOfCars { get; set; }
    }
}

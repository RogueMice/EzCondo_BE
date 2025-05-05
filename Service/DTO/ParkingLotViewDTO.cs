using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.DTO
{
    public class ParkingLotViewDTO
    {
        public Guid ParkingId { get; set; }

        public string Name { get; set; }

        public string Apartment { get; set; }

        public int NumberOfMotorbike { get; set; }

        public int NumberOfCar { get; set; }

        public bool Accept { get; set; }

        public int Total => NumberOfMotorbike + NumberOfCar;
    }
}

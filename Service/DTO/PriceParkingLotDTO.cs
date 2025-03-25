using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.DTO
{
    public class PriceParkingLotDTO
    {
        public Guid? Id { get; set; }

        public decimal? PricePerMotor { get; set; }

        public decimal? PricePerOto { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.DTO
{
    public class PriceElectricTierDTO
    {
        public Guid? Id { get; set; }

        public decimal MinKWh { get; set; }

        public decimal MaxKWh { get; set; }

        public decimal PricePerKWh { get; set; }
    }
}

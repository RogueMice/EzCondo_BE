using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.DTO
{
    public class PriceWaterTierDTO
    {
        public Guid? Id { get; set; }

        public decimal PricePerM3 { get; set; }
    }
}

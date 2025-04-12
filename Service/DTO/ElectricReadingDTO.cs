using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.DTO
{
    public class ElectricReadingDTO
    {
        public Guid Id { get; set; } // done

        public Guid ElectricMetersId { get; set; } // don't have

        public DateTime? ReadingDate { get; set; } // don't have

        public decimal PreElectricNumber { get; set; } // don't have

        public decimal CurrentElectricNumber { get; set; } // done

        public decimal Consumption { get; set; } // don't have
    }
}

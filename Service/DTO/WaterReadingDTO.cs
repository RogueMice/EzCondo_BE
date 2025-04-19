using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.DTO
{
    public class WaterReadingDTO
    {
        public Guid Id { get; set; }

        public Guid WaterMetersId { get; set; }

        public DateTime ReadingDate { get; set; }

        public decimal PreWaterNumber { get; set; }

        public decimal CurrentWaterNumber { get; set; }

        public decimal Consumption { get; set; }
    }
}

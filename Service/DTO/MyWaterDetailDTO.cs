using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.DTO
{
    public class MyWaterDetailDTO
    {
        public Guid WaterBillId { get; set; } 

        public string FullName { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        public string ApartmentNumber { get; set; }

        public string MeterNumber { get; set; } = null!;

        public DateTime? ReadingCurrentDate { get; set; }

        public DateTime? ReadingPreDate { get; set; }

        public decimal Pre_water_number { get; set; }

        public decimal Current_water_number { get; set; }

        public decimal Consumption { get; set; }

        public decimal Price { get; set; }

        public string Status { get; set; }

        public DateTime? PaymentTerm { get; set; }
    }
}

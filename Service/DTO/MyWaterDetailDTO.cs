using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.DTO
{
    public class MyWaterDetailDTO
    {
        public string FullName { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        public string ApartmentNumber { get; set; }

        public string MeterNumber { get; set; } = null!;

        public DateTime? readingDate { get; set; }

        public decimal pre_water_number { get; set; }

        public decimal current_water_number { get; set; }

        public decimal consumption { get; set; }

        public decimal price { get; set; }

        public string status { get; set; }
    }
}

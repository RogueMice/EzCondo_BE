using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.DTO
{
    public class ElectricDetailDTO
    {
        public string FullName { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        public string ApartmentNumber { get; set; }

        public string MeterNumber { get; set; } = null!;

        public DateTime? ReadingPreDate { get; set; }

        public DateTime? ReadingCurrentDate { get; set; } 

        public decimal Pre_electric_number { get; set; }

        public decimal Current_electric_number { get; set; }

        public decimal Consumption { get; set; }

        public decimal Price { get; set; }
    }
}

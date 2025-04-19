using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.DTO
{
    public class WaterViewDTO
    {
        public string FullName { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        public string ApartmentNumber { get; set; }

        public decimal Consumption { get; set; }

        public DateTime? ReadingDate { get; set; }

        public string status { get; set; } // Payment or not
    }
}

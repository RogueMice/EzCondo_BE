using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.DTO
{
    public class WaterViewDTO
    {
        public Guid Id { get; set; }

        public Guid WaterBillId { get; set; }

        public string FullName { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        public string ApartmentNumber { get; set; }

        public decimal Consumption { get; set; }

        public DateTime? ReadingPreDate { get; set; }

        public DateTime? ReadingCurrentDate { get; set; }

        public string Status { get; set; } // Payment or not
    }
}

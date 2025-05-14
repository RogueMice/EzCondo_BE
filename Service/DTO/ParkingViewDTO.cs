using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.DTO
{
    public class ParkingViewDTO
    {
        public Guid ParkingId { get; set; }

        public string FullName { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        public string ApartmentNumber { get; set; }

        public decimal Amount { get; set; }

        public string Status { get; set; } // Payment or not

        public DateTime CreateDate { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.DTO
{
    public class BookingViewDTO
    {
        public Guid Id { get; set; }

        public string ApartmentNumber { get; set; }

        public string FullName { get; set; }

        public string ServiceName { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public decimal Price { get; set; }

        public DateTime BookingDate { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.DTO
{
    public class MyBookingViewDTO
    {
        public Guid Id { get; set; }

        public Guid ServiceId { get; set; }

        public string ServiceName { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string Status { get; set; } = null!;

        public decimal Price { get; set; }

        public DateTime CreateDate { get; set; }


        public string Method { get; set; }

    }
}

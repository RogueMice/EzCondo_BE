using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.DTO
{
    public class MyPaymentViewDTO
    {
        public Guid Id { get; set; }

        public string ApartmentNumber { get; set; }

        public string FullName { get; set; }

        public decimal Amount { get; set; }

        public string Type { get; set; }

        public DateTime CreateDate { get; set; }

        public string Status { get; set; }
    }
}

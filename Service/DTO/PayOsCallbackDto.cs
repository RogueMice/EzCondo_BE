using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.DTO
{
    public class PayOsCallbackDto
    {
        public Guid PaymentId { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }
    }
}

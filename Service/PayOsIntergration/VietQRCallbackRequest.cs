using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.PayOsIntergration
{
    public class VietQRCallbackRequest
    {
        public string Status { get; set; } // "success" or "failed"
        public string TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string AddInfo { get; set; }
    }
}

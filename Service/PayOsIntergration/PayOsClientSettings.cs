using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.PayOsIntergration
{
    public class PayOsClientSettings
    {
        public string ClientID { get; set; } = null!;
        public string ApiKey { get; set; } = null!;
        public string ChecksumKey { get; set; } = null!;
    }
}

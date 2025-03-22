using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.DTO
{
    public class NotificationViewListDTO
    {
        public int Total { get; set; }
        public List<NotificationViewDTO> Notifications { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.DTO
{
    public class MarkAsReadRequestDTO
    {
        public List<Guid> NotificationIds { get; set; }
    }
}

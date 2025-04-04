using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.DTO
{
    public class UpdateIncidentDTO
    {
        public Guid IncidentId { get; set; }

        public string Status { get; set; } = null!;
    }
}

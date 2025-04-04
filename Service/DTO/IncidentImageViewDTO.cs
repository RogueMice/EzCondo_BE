using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.DTO
{
    public class IncidentImageViewDTO
    {
        public Guid Id { get; set; }

        public Guid IncidentId { get; set; }

        public string ImgPath { get; set; }
    }
}

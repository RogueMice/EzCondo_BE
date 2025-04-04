using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.DTO
{
    public class IncidentImageDTO
    {
        public Guid? Id { get; set; }

        public Guid IncidentId { get; set; }

        public List<IFormFile> Images { get; set; }
    }
}

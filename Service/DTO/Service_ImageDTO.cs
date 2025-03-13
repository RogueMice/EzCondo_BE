using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.DTO
{
    public class Service_ImageDTO
    {
        public Guid? id { get; set; }

        public Guid service_Id { get; set; }
        public List<IFormFile> serviceImages { get; set; }
    }
}

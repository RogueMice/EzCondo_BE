using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.DTO
{
    public class ServiceImageViewDTO
    {
        public Guid Id { get; set; }

        public Guid ServiceId { get; set; }

        public string ImgPath { get; set; } 
    }
}

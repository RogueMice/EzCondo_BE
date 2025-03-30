using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace EzConDo_Service.DTO
{
    public class NotificationImageDTO
    {
        public Guid? Id { get; set; }

        public Guid NotificationId { get; set; } 

        public List<IFormFile> Image { get; set; } = null!;
    }
}

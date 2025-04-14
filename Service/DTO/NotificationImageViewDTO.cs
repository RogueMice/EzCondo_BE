using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.DTO
{
    public class NotificationImageViewDTO
    {
        public Guid? Id { get; set; }

        public Guid NotificationId { get; set; }

        public string Image { get; set; } = null!;
    }
}

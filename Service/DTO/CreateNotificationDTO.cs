using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.DTO
{
    public class CreateNotificationDTO
    {
        public Guid? Id { get; set; }

        public string Title { get; set; } = null!;

        public string Content { get; set; } = null!;

        public string Type { get; set; } = null!;

        public Guid? CreatedBy { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}

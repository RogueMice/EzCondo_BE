using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.DTO
{
    public class UpdateFcmTokenDTO
    {
        public string FcmToken { get; set; } = null!;

        public string Type { get; set; } = null!;

        public bool? IsActive { get; set; }

        public Guid? UserId { get; set; }
    }
}

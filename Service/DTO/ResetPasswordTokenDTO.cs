using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.DTO
{
    public class ResetPasswordTokenDTO
    {
        public string TokenMemory { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }
}

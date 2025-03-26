using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.DTO
{
    public class VerifyOTP_DTO
    {
        [EmailAddress]
        public string Email { get; set; } = null!;
        public string Code { get; set; } = null!;
    }
}

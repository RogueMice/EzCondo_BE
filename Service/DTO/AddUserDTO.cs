using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.DTO
{
    public class AddUserDTO
    {

        public string FullName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string PhoneNumber { get; set; } = null!;

        public DateTime? DateOfBirth { get; set; }

        public string Gender { get; set; } = null!;

        public string? Password { get; set; } 

        public string? Status { get; set; }

        public DateTime? CreateAt { get; set; }

        public DateTime? UpdateAt { get; set; }

        public string RoleName { get; set; } = null!;

        public string ApartmentNumber { get; set; } = null!;
    }
}

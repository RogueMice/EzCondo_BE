using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.DTO
{
    public class UserInforDTO
    {
        public Guid Id { get; set; }

        public string FullName { get; set; } = null!;

        public DateTime? DateOfBirth { get; set; }

        public string Gender { get; set; } = null!;

        public string ApartmentNumber { get; set; } = null!;

        public string PhoneNumber { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string Status { get; set; } = null!;

        public string RoleName { get; set; } = null!;
    }
}

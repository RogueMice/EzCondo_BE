using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.DTO
{
    public class UpdateUserDTO
    {
        public Guid id { get; set; }

        public string fullName { get; set; } = null!;

        public DateTime dateOfBirth { get; set; }

        public string gender { get; set; } = null!;

        public string apartmentNumber { get; set; } = null!;

        public string phoneNumber { get; set; } = null!;

        public string status { get; set; } = null!;
    }
}

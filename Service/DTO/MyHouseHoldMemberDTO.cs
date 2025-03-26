using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.DTO
{
    public class MyHouseHoldMemberDTO
    {
        public Guid? Id { get; set; }

        public string No { get; set; } = null!;

        public string FullName { get; set; } = null!;

        public DateOnly? DateOfBirth { get; set; }

        public string Gender { get; set; } = null!;

        public string PhoneNumber { get; set; } = null!;

        public string Relationship { get; set; } = null!;
    }
}

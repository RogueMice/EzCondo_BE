using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.DTO
{
    public class BookingDTO
    {
        public Guid? Id { get; set; }

        public Guid ServiceId { get; set; }

        public Guid? UserId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string ForMonthOrYear { get; set; } = null!;

        public string? Status { get; set; } 
    }
}

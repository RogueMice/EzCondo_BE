using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.DTO
{
    public class IncidientDTO
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string Title { get; set; } = null!;

        public string Description { get; set; } = null!;

        public DateTime ReportedAt { get; set; }

        public string Status { get; set; } = null!;

        public int Priority { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}

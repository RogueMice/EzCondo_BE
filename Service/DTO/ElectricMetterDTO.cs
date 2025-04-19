using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.DTO
{
    public class ElectricMetterDTO
    {
        public Guid? Id { get; set; }

        public string MeterNumber { get; set; } = null!;

        public DateOnly InstallationDate { get; set; }

        public Guid ApartmentId { get; set; }

        public string? ApartmentNumber { get; set; }
    }
}

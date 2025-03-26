using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.DTO
{
    public class ApartmentViewDTO
    {
        public Guid Id { get; set; }

        public string ApartmentNumber { get; set; } = null!;

        public int ResidentNumber { get; set; }

        public decimal Acreage { get; set; }

        public string? Description { get; set; }
    }
}

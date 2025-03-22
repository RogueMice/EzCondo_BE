using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.DTO
{
    public class ServiceViewDTO
    {
        public Guid Id { get; set; }

        public string ServiceName { get; set; } = null!;

        public string Description { get; set; } = null!;

        public bool? TypeOfMonth { get; set; }

        public bool? TypeOfYear { get; set; }

        public decimal? PriceOfMonth { get; set; }

        public decimal? PriceOfYear { get; set; }

        public string Status { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}

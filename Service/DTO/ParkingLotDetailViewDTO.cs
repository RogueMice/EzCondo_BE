using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.DTO
{
    public class ParkingLotDetailViewDTO
    {
        public Guid Id { get; set; }

        public string Type { get; set; } = null!;

        public string Status { get; set; } = null!;

        public string Checking { get; set; } = null!;

        public decimal Price { get; set; }
    }
}

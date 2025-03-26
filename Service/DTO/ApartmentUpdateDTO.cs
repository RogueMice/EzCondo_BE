using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.DTO
{
    public class ApartmentUpdateDTO
    {
        public Guid Id { get; set; }
        public decimal Acreage { get; set; }
        public string? Description { get; set; }

    }
}

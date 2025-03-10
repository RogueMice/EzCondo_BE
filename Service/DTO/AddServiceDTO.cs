using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.DTO
{
    public class AddServiceDTO
    {
        public Guid? Id { get; set; }

        public string ServiceName { get; set; } = null!;

        public string Description { get; set; } = null!;

        public decimal Cost { get; set; }

        public string? Status { get; set; } 

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}

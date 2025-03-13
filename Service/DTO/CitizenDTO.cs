using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.DTO
{
    public class CitizenDTO
    {
        public Guid userId { get; set; }

        public string no { get; set; } = null!;

        public DateOnly dateOfIssue { get; set; }

        public DateOnly dateOfExpiry { get; set; }

        public IFormFile? frontImage { get; set; }

        public IFormFile? backImage { get; set; }
    }
}

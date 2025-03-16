using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.DTO
{
    public class CitizenViewDTO
    {
        public Guid UserId { get; set; }

        public string No { get; set; } = null!;

        public DateOnly DateOfIssue { get; set; }

        public DateOnly DateOfExpiry { get; set; }

        public string? FrontImage { get; set; }

        public string? BackImage { get; set; }
    }
}

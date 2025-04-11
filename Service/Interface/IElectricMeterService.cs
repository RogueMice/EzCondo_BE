using EzConDo_Service.DTO;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.Interface
{
    public interface IElectricMeterService
    {
        Task<List<ElectricMetterDTO>> AddElectricMetters(IFormFile file);
    }
}

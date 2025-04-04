using EzConDo_Service.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.Interface
{
    public interface I_incidentService
    {
        Task<Guid?> AddAsync(IncidentDTO dto, Guid userId);

        Task<string> EditAsync(UpdateIncidentDTO dto);

        Task<List<IncidentDTO>> GetIncidentsAsync();

        Task<IncidentDTO> GetIncidentByUserIdAsync(Guid userId);
    }
}

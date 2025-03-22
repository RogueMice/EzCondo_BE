using EzConDo_Service.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.Interface
{
    public interface IService_ImageService
    {
        Task<string?> AddOrUpdateServiceImagesAsync(Service_ImageDTO serviceImageDTO);

        Task<List<ServiceImageViewDTO>> GetServiceImagesAsync(Guid serviceId);

    }
}

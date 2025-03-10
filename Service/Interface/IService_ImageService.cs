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
        Task AddServiceImagesAsync(Service_ImageDTO serviceImageDTO);
    }
}

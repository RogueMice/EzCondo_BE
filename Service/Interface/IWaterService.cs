using EzConDo_Service.DTO;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.Interface
{
    public interface IWaterService
    {
        Task<byte[]> CreateTemplateWaterMetterAsync();

        Task<byte[]> CreateTemplateWaterReadingAsync();

        Task<List<WaterMetterDTO>> AddWaterMettersAsync(IFormFile file);

        Task<Guid?> AddWaterBillAsync(Guid waterReadingId);

        Task<List<WaterReadingDTO>> AddWaterReadingAsync(IFormFile file);

        Task<List<WaterMetterDTO>> GetAllWaterMettersAsync();

        Task<List<WaterReadingDTO>> GetAllWaterReadingsAsync();

        Task<List<WaterViewDTO>> GetAllWaterAsync(bool? status, int? day, int? month);

        Task<WaterDetailDTO> GetWaterDetailAsync(Guid electricReadingId);

        Task<List<MyWaterDetailDTO>> GetMyWaterDetailAsync(Guid userId, bool? status);

        Task<string> UpdateWaterBillsAsync(List<UpdateWaterBillDTO> dtos);
    }
}

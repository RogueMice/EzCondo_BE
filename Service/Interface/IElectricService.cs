﻿using EzCondo_Data.Context;
using EzConDo_Service.DTO;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.Interface
{
    public interface IElectricService
    {
        Task<List<ElectricMetterDTO>> AddElectricMettersAsync(IFormFile file);

        Task<List<ElectricMetterDTO>> GetAllElectricMettersAsync();

        Task<List<ElectricReadingDTO>> AddElectricReadingAsync(IFormFile file);

        Task<List<ElectricReadingDTO>> GetAllElectricReadingsAsync();

        Task<List<ElectricViewDTO>> GetAllElectricAsync(bool? status, int? day, int? month);

        Task<ElectricDetailDTO> GetElectricDetailAsync(Guid electricReadingId);

        Task<List<MyElectricDetailDTO>> GetMyElectricDetailAsync(Guid userId, bool? status);

        Task<Guid?> AddElectricBillAsync(Guid electricReadingId);

        Task<byte[]> CreateTemplateElectricMetterAsync();

        Task<byte[]> CreateTemplateElectricReadingAsync();

        Task<string> UpdateElectricBillsAsync(List<UpdateElectricBillDTO> dtos);

        Task UpdateOverdueElectricBillsAsync();
    }
}

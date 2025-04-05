using EzConDo_Service.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.Interface
{
    public interface IApartmentService
    {
        Task <ApartmentViewDTO> GetMyApartmentAsync(Guid userId);

        Task<List<ApartmentViewDTO>> GetApartmentAsync(Guid? userId, string? apartmentNumber, bool? status);

        Task<ApartmentViewDTO> GetApartmentByIdAsync(Guid apartmentId);

        Task<Guid?> AddApartmentAsync(ApartmentViewDTO apartmentDto);

        Task<string?> UpdateApartmentAsync(ApartmentUpdateDTO apartmentDto);
    }
}

using EzConDo_Service.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.Interface
{
    public interface IBookingService
    {
        Task<Guid> AddBookingAsync(BookingDTO dto);

        Task<List<BookingViewDTO>> GetAllBookingsAsync(string? search, int? month);
    }
}

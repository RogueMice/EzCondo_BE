using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.Interface
{
    public interface IPaymentService
    {
        Task<string> CreatePaymentForBookingAsync(Guid bookingId);
    }
}

using EzConDo_Service.DTO;
using Net.payOS.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EzConDo_Service.Interface
{
    public interface IPaymentService
    {
        Task<object> CreatePaymentForBookingAsync(Guid bookingId);

        Task<bool> HandleWebHookAsync(WebhookType body);

        Task<bool> CheckPaymentAsync(Guid paymentId);
    }
}

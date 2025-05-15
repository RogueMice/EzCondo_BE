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

        Task<object> CreatePaymentForElectricAsync(Guid electricBillId, Guid userId);

        Task<object> CreatePaymentForWaterAsync(Guid waterBillId, Guid userId);

        Task<object> CreatePaymentForParkingAsync(Guid paymentId, Guid userId);

        Task<List<PaymentViewDTO>> GetAllPaymentsAsync(string? search, int? month);

        Task<List<MyPaymentViewDTO>> GetMyPaymentsAsync(Guid userId);

        Task<List<MyPaymentViewDTO>> GetNeedMyPaymentsAsync(Guid userId);
    }
}

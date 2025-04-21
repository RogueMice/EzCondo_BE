using EzCondo_Data.Context;
using EzCondo_Data.Domain;
using EzConDo_Service.Interface;
using EzConDo_Service.PayOsIntergration;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EzConDo_Service.ExceptionsConfig.CustomException;

namespace EzConDo_Service.Implement
{
    public class PaymentService: IPaymentService
    {
        private readonly ApartmentDbContext dbContext;
        private readonly PayQrSettings _settings;

        public PaymentService(ApartmentDbContext dbContext, IOptions<PayQrSettings> options)
        {
            this.dbContext = dbContext;
            _settings = options.Value;
        }

        public async Task<string> CreatePaymentForBookingAsync(Guid bookingId)
        {
            var booking = dbContext.Bookings
                .Include(b => b.Service)
                .FirstOrDefault(b => b.Id == bookingId)
                ?? throw new NotFoundException($"BookingId {bookingId} is not found!");

            if (booking.Status.ToLower() != "pending")
            {
                throw new BadRequestException($"BookingId {bookingId} is not in a valid state for payment.");
            }

            //Get price's month or year
            int month = booking.EndDate.Month - booking.StartDate.Month;
            int year = booking.EndDate.Year - booking.StartDate.Year;
            decimal price = 0;
            //month price
            if (month != 0 && year == 0)
            {
                price += booking.Service.PriceOfMonth.Value *month;
            }
            if (month == 0 && year != 0)
            {
                price += booking.Service.PriceOfYear.Value *year;
            }

            //Create payment
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                UserId = booking.UserId,
                BookingId = booking.Id,
                Amount = price,
                Status = "pending",
                Method = "VietQR",
                CreateDate = DateTime.UtcNow
            };
            await dbContext.AddAsync(payment);
            await dbContext.SaveChangesAsync();

            //generate QR code
            var qrCode = await GenerateQRCode(price, $"Thanh toán tiền cho dịch vụ {booking.Service.ServiceName}");
            return qrCode.ToString();
        }

        public async Task<string> GenerateQRCode(decimal amount, string description)
        {
            description ??= string.Empty;
            var baseUri = $"{_settings.VietQRBaseUrl.TrimEnd('/')}/{_settings.BankCode.Trim()}";
            var queryParams = new Dictionary<string, string>
            {
                ["accountName"] = _settings.AccountName.Trim(),
                ["amount"] = amount.ToString(),
                ["addInfo"] = description
            };
            string qrUrl = QueryHelpers.AddQueryString(baseUri, queryParams);
            return qrUrl;
        }
    }
}

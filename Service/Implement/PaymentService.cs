using EzCondo_Data.Context;
using EzCondo_Data.Domain;
using EzConDo_Service.DTO;
using EzConDo_Service.Interface;
using EzConDo_Service.PayOsIntergration;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Options;
using Net.payOS.Types;
using Net.payOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EzConDo_Service.ExceptionsConfig.CustomException;
using DocumentFormat.OpenXml.Wordprocessing;
using CloudinaryDotNet;
using System.Text.Json;
using Azure;

namespace EzConDo_Service.Implement
{
    public class PaymentService : IPaymentService
    {
        private readonly ApartmentDbContext dbContext;
        private readonly PayOsClientSettings _setting2;
        private readonly PayQrSettings _settings;

        public PaymentService(ApartmentDbContext dbContext, IOptions<PayQrSettings> options, IOptions<PayOsClientSettings> option)
        {
            this.dbContext = dbContext;
            _setting2 = option.Value;
            _settings = options.Value;
        }

        public async Task<object> CreatePaymentForBookingAsync(Guid bookingId)
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
                price += booking.Service.PriceOfMonth.Value * month;
            }
            if (month == 0 && year != 0)
            {
                price += booking.Service.PriceOfYear.Value * year;
            }

            //Create payment
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                UserId = booking.UserId,
                BookingId = booking.Id,
                Amount = 5000, 
                Status = "pending",
                Method = "VietQR",
                CreateDate = DateTime.UtcNow
            };
            await dbContext.AddAsync(payment);
            await dbContext.SaveChangesAsync();

            //generate QR code
            return await CreatePaymentLink(payment.Id, (int)payment.Amount, booking.Service.ServiceName);
        }

        public async Task<object> CreatePaymentLink(Guid paymentId, int amount, string serviceName)
        {
            PayOS payOS = new PayOS(_setting2.ClientID, _setting2.ApiKey, _setting2.ChecksumKey);
            ItemData item = new ItemData(serviceName, 1, amount);
            List<ItemData> items = new List<ItemData>();
            items.Add(item);

            PaymentData paymentData = new PaymentData(
                Math.Abs(paymentId.GetHashCode() & 0x7FFFFFFF),
                amount,
                "Thanh toán dịch vụ "+ serviceName,
                items,
                "Payment pailure",
                "Payment success "
            );

            CreatePaymentResult createPayment = await payOS.createPaymentLink(paymentData);

            var result = new
            {
                QrCode = createPayment.qrCode,
                Amount = createPayment.amount + "  " + createPayment.currency,
                Description = createPayment.description,
                AccountNumber = createPayment.accountNumber,
                AccountOwner = _settings.AccountName,
                PaymentId = paymentId.ToString()
            };

            // Update transactionId with null-check to fix CS8602
            var payment = await dbContext.Payments.FirstOrDefaultAsync(p => p.Id == paymentId);
            payment.TransactionId = createPayment.paymentLinkId;
            await dbContext.SaveChangesAsync();
            return result;
        }

        public async Task<bool> HandleWebHookAsync(WebhookType body)
        {
            var payment = await dbContext.Payments
                .Include(p => p.Booking)
                .FirstOrDefaultAsync(p => p.TransactionId == body.data.paymentLinkId);

            if (payment == null)
                return false;

            var newStatus = body.success ? "completed" : "failed";

            payment.Status = newStatus;
            if (payment.Booking != null)
                payment.Booking.Status = newStatus;

            await dbContext.SaveChangesAsync();

            return body.success;
        }

        public async Task<bool> CheckPaymentAsync(Guid paymentId)
        {
            var check = await dbContext.Payments.AsNoTracking().FirstOrDefaultAsync(p => p.Id == paymentId);
            if (check.Status.ToLower() == "completed")
                return true;
            return false;
        }
    }
}

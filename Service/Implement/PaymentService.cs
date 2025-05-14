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
using DocumentFormat.OpenXml.Bibliography;

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

            var existingPayment = await dbContext.Payments
                .FirstOrDefaultAsync(p => p.BookingId == bookingId && p.Status == "pending");

            if (existingPayment != null)
            {
                return await CreatePaymentLink(existingPayment.Id, (int)existingPayment.Amount, booking.Service.ServiceName);
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

        public async Task<object> CreatePaymentForElectricAsync(Guid electricBillId, Guid userId)
        {
            var electricBill = await dbContext.ElectricBills
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == electricBillId)
                ?? throw new NotFoundException($"ElectricBillId {electricBillId} is not found!");

            if (!string.Equals(electricBill.Status, "pending", StringComparison.OrdinalIgnoreCase))
            {
                throw new BadRequestException($"ElectricBillId {electricBillId} is not in a valid state for payment.");
            }

            var existingPayment = await dbContext.Payments
                .FirstOrDefaultAsync(p => p.ElectricBillId == electricBillId && p.Status == "pending");

            if (existingPayment != null)
            {
                return await CreatePaymentLink(existingPayment.Id, (int)existingPayment.Amount, "dien");
            }

            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ElectricBillId = electricBillId,
                Amount = electricBill.TotalAmount,
                Status = "pending",
                Method = "VietQR",
                CreateDate = DateTime.UtcNow
            };

            await dbContext.Payments.AddAsync(payment);
            await dbContext.SaveChangesAsync();

            var result = await CreatePaymentLink(payment.Id, (int)payment.Amount, "dien");

            return result;
        }

        public async Task<object> CreatePaymentForWaterAsync(Guid waterBillId, Guid userId)
        {
            var waterBill = dbContext.WaterBills
                .FirstOrDefault(b => b.Id == waterBillId)
                ?? throw new NotFoundException($"WaterBillId {waterBillId} is not found!");

            if (waterBill.Status.ToLower() != "pending")
            {
                throw new BadRequestException($"WaterBillId {waterBillId} is not in a valid state for payment.");
            }

            var existingPayment = await dbContext.Payments
                .FirstOrDefaultAsync(p => p.WaterBillId == waterBillId && p.Status == "pending");

            if (existingPayment != null)
            {
                return await CreatePaymentLink(existingPayment.Id, (int)existingPayment.Amount, "nuoc");
            }

            //Create payment
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                WaterBillId = waterBillId,
                Amount = 5000,//waterBill.TotalAmount, set default 5k nhớ sửa
                Status = "pending",
                Method = "VietQR",
                CreateDate = DateTime.UtcNow
            };
            await dbContext.AddAsync(payment);
            await dbContext.SaveChangesAsync();

            //generate QR code
            return await CreatePaymentLink(payment.Id, (int)payment.Amount, "nuoc");
        }

        public async Task<object> CreatePaymentForParkingAsync(Guid parkingId, Guid userId)
        {
            var payment = dbContext.Payments
                .FirstOrDefault(p => p.ParkingId == parkingId)
                ?? throw new NotFoundException($"Id {parkingId} is not found!");

            //generate QR code

            return await CreatePaymentLink(payment.Id, 5000, "do xe");  //set default 5k
        }

        public async Task<object> CreatePaymentLink(Guid paymentId, int amount, string serviceName)
        {
            PayOS payOS = new PayOS(_setting2.ClientID, _setting2.ApiKey, _setting2.ChecksumKey);
            ItemData item = new ItemData(serviceName, 1, amount);
            List<ItemData> items = new List<ItemData>();
            items.Add(item);

            Guid newPaymentId = Guid.NewGuid();
            PaymentData paymentData = new PaymentData(
                Math.Abs(newPaymentId.GetHashCode() & 0x7FFFFFFF),
                amount,
                "Thanh toan tien "+ serviceName,
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
                .Include(p => p.ElectricBill)
                .Include(p => p.WaterBill)
                .FirstOrDefaultAsync(p => p.TransactionId == body.data.paymentLinkId);

            if (payment == null)
                return false;

            var newStatus = body.success ? "completed" : "failed";

            payment.Status = newStatus;
            if (payment.Booking != null)
                payment.Booking.Status = newStatus;
            else if (payment.ElectricBill != null)
                payment.ElectricBill.Status = newStatus;
            else if (payment.WaterBill != null)
                payment.WaterBill.Status = newStatus;
            else if (payment.ParkingId != null)
                payment.Status = newStatus;


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

        public async Task<List<PaymentViewDTO>> GetAllPaymentsAsync(string? search, int? month)
        {
            var query = dbContext.Payments
                .Include(p => p.User)
                    .ThenInclude(u => u.Apartments)     
                .Include(p => p.Booking)
                .Include(p => p.ElectricBill)
                .Include(p => p.WaterBill)
                .Include(p => p.Parking)
                .Where(p => p.Status == "completed")
                .AsQueryable();

            //Filter for month 
            if (month.HasValue && month.Value >= 1 && month.Value <= 12)
            {
                query = query.Where(p => p.CreateDate.Month == month.Value);
            }

            //Filter for search: Find on Name or ApartmentNumber
            if (!string.IsNullOrWhiteSpace(search))
            {
                var key = search.Trim().ToLower();
                query = query.Where(p =>
                    p.User.FullName.ToLower().Contains(key)
                    || p.User.Apartments.Any(a => a.ApartmentNumber.ToLower().Contains(key))
                    // search for Type
                    || (p.Booking != null && "booking".Contains(key))
                    || (p.ElectricBill != null && "electric".Contains(key))
                    || (p.WaterBill != null && "water".Contains(key))
                    || (p.Parking != null && "parking".Contains(key)));
            }

            var result = await query
                .OrderByDescending(p => p.CreateDate)
                .Select(p => new PaymentViewDTO
                {
                    Id = p.Id,
                    FullName = p.User.FullName,
                    ApartmentNumber = p.User.Apartments.FirstOrDefault().ApartmentNumber,
                    Amount = p.Amount,
                    CreateDate = p.CreateDate,
                    Type = p.Booking != null ? "Booking"
                         : p.ElectricBill != null ? "Electric"
                         : p.WaterBill != null ? "Water"
                         : p.Parking != null ? "Parking"
                         : "Other"
                })
                .ToListAsync();

            return result;
        }

        public async Task<List<MyPaymentViewDTO>> GetMyPaymentsAsync(Guid userId)
        {
            var query = dbContext.Payments
                .Include(p => p.User)
                    .ThenInclude(u => u.Apartments)
                .Include(p => p.Booking)
                .Include(p => p.ElectricBill)
                .Include(p => p.WaterBill)
                .Include(p => p.Parking)
                .Where(p => p.UserId == userId)
                .AsQueryable();

            var result = await query
                .OrderByDescending(p => p.CreateDate)
                .Select(p => new MyPaymentViewDTO
                {
                    Id = p.Id,
                    FullName = p.User.FullName,
                    ApartmentNumber = p.User.Apartments.FirstOrDefault().ApartmentNumber,
                    Amount = p.Amount,
                    CreateDate = p.CreateDate,
                    Type = p.Booking != null ? "Booking"
                         : p.ElectricBill != null ? "Electric"
                         : p.WaterBill != null ? "Water"
                         : p.Parking != null ? "Parking"
                         : "Other",
                    Status = p.Status
                })
                .ToListAsync();
            return result;
        }
    }
}

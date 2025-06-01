using DocumentFormat.OpenXml.InkML;
using EzCondo_Data.Context;
using EzCondo_Data.Domain;
using EzConDo_Service.DTO;
using EzConDo_Service.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EzConDo_Service.ExceptionsConfig.CustomException;

namespace EzConDo_Service.Implement
{
    public class BookingService : IBookingService
    {
        private readonly ApartmentDbContext dbContext;

        public BookingService(ApartmentDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Guid> AddBookingAsync(BookingDTO dto)
        {
            bool serviceExists = await dbContext.Services.AnyAsync(s => s.Id == dto.ServiceId);
            if (!serviceExists)
                throw new NotFoundException("Service not found.");
            switch(dto.ForMonthOrYear.ToLower())
            {
                case "month":
                    dto.EndDate = dto.StartDate.AddMonths((int)dto.TotalMonth);
                    break;
                case "year":
                    dto.StartDate = DateTime.UtcNow;
                    dto.EndDate = dto.StartDate.AddYears(1);
                    break;
                default:
                    throw new BadRequestException("Invalid ForMonthOrYear value. It should be either 'month' or 'year'.");
            }

            var booking = await dbContext.Bookings.FirstOrDefaultAsync(b => b.ServiceId == dto.ServiceId && b.UserId == dto.UserId);

            //create new booking
            if (booking == null)
            {
                booking = new Booking
                {
                    Id = Guid.NewGuid(),
                    ServiceId = dto.ServiceId,
                    UserId = dto.UserId.Value,
                    StartDate = DateTime.UtcNow,
                    EndDate = dto.EndDate ?? throw new ConflictException("EndDate cannot be null."),
                    Status = "pending"
                };
                await dbContext.Bookings.AddAsync(booking);
            }
            else
            {
                if(booking.Status.ToLower() == "in_use")
                    throw new ConflictException("You are using this service already !");
            }

            await dbContext.SaveChangesAsync();
            return booking.Id;
        }

        public async Task<List<BookingViewDTO>> GetAllBookingsAsync(string? search, int? month)
        {
            var query = dbContext.Bookings
                .Include(p => p.User)
                    .ThenInclude(u => u.Apartments)
                .Include(p => p.Payments)
                .Include(p => p.Service)
                .Where(p => p.Status == "in_use" || p.Status == "completed")
                .AsQueryable();

            // Filter for month
            if (month.HasValue && month.Value >= 1 && month.Value <= 12)
            {
                query = query.Where(p => p.Payments.Any(pm => pm.CreateDate.Month == month.Value));
            }

            // Filter for search: Find on Name or ApartmentNumber
            if (!string.IsNullOrWhiteSpace(search))
            {
                var key = search.Trim().ToLower();
                query = query.Where(p =>
                    p.User.FullName.ToLower().Contains(key)
                    || p.User.Apartments.Any(a => a.ApartmentNumber.ToLower().Contains(key))
                    || p.Service.ServiceName.ToLower().Contains(key)
                );
            }

            var result = await query
                .OrderByDescending(p => p.StartDate)
                .Select(p => new BookingViewDTO
                {
                    Id = p.Id,
                    FullName = p.User.FullName,
                    ApartmentNumber = p.User.Apartments.FirstOrDefault().ApartmentNumber,
                    ServiceName = p.Service.ServiceName,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    Price = p.Payments.FirstOrDefault().Amount,
                    BookingDate = p.Payments.FirstOrDefault().CreateDate
                })
                .ToListAsync();

            return result;
        }

        public async Task<List<MyBookingViewDTO>> GetMyBookingAsync(Guid userId)
        {
            var query = dbContext.Bookings
                .Include(b => b.Service)
                .Where(b => b.UserId == userId
                && (b.Status.ToLower() == "completed" || b.Status == "in_use"));

            var result = await query
                .OrderByDescending(b => b.StartDate)
                .Select(b => new MyBookingViewDTO
                {
                    Id = b.Id,
                    ServiceName = b.Service.ServiceName,
                    ServiceId = b.ServiceId,
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
                    Status = b.Status,
                    Price = b.Payments
                           .OrderByDescending(p => p.CreateDate)
                           .Select(p => p.Amount)
                           .FirstOrDefault(),
                    CreateDate = b.Payments.Cast<Payment>()
                           .OrderByDescending(p => p.CreateDate)
                           .Select(p => p.CreateDate)
                           .FirstOrDefault(),
                    Method = b.Payments.Cast<Payment>()
                           .OrderByDescending(p => p.CreateDate)
                           .Select(p => p.Method)
                           .FirstOrDefault()

                })
                .ToListAsync();
            return result;
        }

        public async Task MarkExpiredBookingsAsCompletedAsync()
        {
            var now = DateTime.UtcNow;

            var expiredBookings = await dbContext.Bookings
                .Where(b => b.EndDate < now && b.Status != "completed")
                .ToListAsync();

            if (!expiredBookings.Any())
                return;

            foreach (var booking in expiredBookings)
            {
                booking.Status = "completed";
            }

            await dbContext.SaveChangesAsync();
        }
    }
}

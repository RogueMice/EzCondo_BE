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
            // Check if Service exists
            bool serviceExists = await dbContext.Services.AnyAsync(s => s.Id == dto.ServiceId);
            if (!serviceExists)
                throw new NotFoundException("Service not found.");
            switch(dto.ForMonthOrYear.ToLower())
            {
                case "month":
                    dto.StartDate = DateTime.UtcNow;
                    dto.EndDate = dto.StartDate?.AddMonths((int)dto.TotalMonth);
                    break;
                case "year":
                    dto.StartDate = DateTime.UtcNow;
                    dto.EndDate = dto.StartDate?.AddYears(1);
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
    }
}

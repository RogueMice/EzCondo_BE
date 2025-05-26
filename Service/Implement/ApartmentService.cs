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
    public class ApartmentService : IApartmentService
    {
        private readonly ApartmentDbContext dbContext;

        public ApartmentService(ApartmentDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<ApartmentViewDTO> GetMyApartmentAsync(Guid userId)
        {
            var apartment = await dbContext.Apartments.FirstOrDefaultAsync(x => x.UserId == userId) ?? throw new NotFoundException($"Userid: {userId} is not found !");

            return new ApartmentViewDTO
            {
                Id = apartment.Id,
                ApartmentNumber = apartment.ApartmentNumber,
                ResidentNumber = apartment.ResidentNumber,
                Acreage = apartment.Acreage,
                Description = apartment.Description,
                UserId = apartment.UserId
            };
        }

        public async Task<List<ApartmentViewDTO>> GetApartmentAsync(Guid? userId, string? apartmentNumber, bool? status)
        {
            var query = dbContext.Apartments.AsNoTracking().AsQueryable();

            if (userId.HasValue)
            {
                query = query.Where(x => x.UserId == userId.Value);
            }

            if (!string.IsNullOrEmpty(apartmentNumber))
            {
                query = query.Where(x => x.ApartmentNumber.Contains(apartmentNumber));
            }

            if (status.HasValue)
            {
                if (status.Value)
                    query = query.Where(x => x.UserId != null);
                else
                    query = query.Where(x => x.UserId == null);
            }


            var apartments = await query
                .Select(apartment => new ApartmentViewDTO
                {
                    Id = apartment.Id,
                    ApartmentNumber = apartment.ApartmentNumber,
                    ResidentNumber = apartment.ResidentNumber,
                    Acreage = apartment.Acreage,
                    Description = apartment.Description,
                    UserId = apartment.UserId
                }).ToListAsync();

            return apartments;
        }

        public async Task<string?> UpdateApartmentAsync(ApartmentUpdateDTO apartmentDto)
        {
            var apartment = await dbContext.Apartments.FirstOrDefaultAsync(x => x.Id == apartmentDto.Id) ?? throw new NotFoundException($"ApartmentId: {apartmentDto.Id} is not found !");
            apartment.Acreage = apartmentDto.Acreage;
            apartment.Description = apartmentDto.Description;
            await dbContext.SaveChangesAsync();
            return "Update apartment successfully !";
        }

        public async Task<Guid?> AddApartmentAsync(ApartmentViewDTO apartmentDto)
        {
            var existingApartment = await dbContext.Apartments.FirstOrDefaultAsync(x => x.ApartmentNumber == apartmentDto.ApartmentNumber);

            if (existingApartment != null)
            {
                throw new BadRequestException("Apartment number already exist !");
            }

            var apartment = new Apartment
            {
                Id = Guid.NewGuid(),
                ApartmentNumber = apartmentDto.ApartmentNumber,
                ResidentNumber = 0,
                Acreage = apartmentDto.Acreage,
                Description = apartmentDto.Description,
                UserId = null
            };
            await dbContext.Apartments.AddAsync(apartment);
            dbContext.SaveChanges();
            return apartment.Id;
        }

        public async Task<ApartmentViewDTO> GetApartmentByIdAsync(Guid apartmentId)
        {
            var apartment = await dbContext.Apartments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == apartmentId) ?? throw new NotFoundException($"ApartmentId: {apartmentId} is not found !");
            return new ApartmentViewDTO
            {
                Id = apartment.Id,
                ApartmentNumber = apartment.ApartmentNumber,
                ResidentNumber = apartment.ResidentNumber,
                Acreage = apartment.Acreage,
                Description = apartment.Description,
                UserId = apartment.UserId
            };
        }

        public async Task<GenerateDashboardDTO> GetApartmentDashBoardAsync()
        {
            // Múi giờ Việt Nam (UTC+7)
            var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

            // Lấy ngày hiện tại theo giờ Việt Nam
            var todayVN = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone).Date;

            // Xác định Thứ Hai của tuần này (giờ VN)
            int diffToMonday = ((int)todayVN.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            var startOfThisWeekVN = todayVN.AddDays(-diffToMonday);
            var endOfThisWeekVN = startOfThisWeekVN.AddDays(7); // Exclusive

            // Tuần trước
            var startOfLastWeekVN = startOfThisWeekVN.AddDays(-7);
            var endOfLastWeekVN = startOfThisWeekVN; // Exclusive

            // Chuyển về UTC để so sánh trong database
            var startOfThisWeekUtc = TimeZoneInfo.ConvertTimeToUtc(startOfThisWeekVN, vnTimeZone);
            var endOfThisWeekUtc = TimeZoneInfo.ConvertTimeToUtc(endOfThisWeekVN, vnTimeZone);
            var startOfLastWeekUtc = TimeZoneInfo.ConvertTimeToUtc(startOfLastWeekVN, vnTimeZone);
            var endOfLastWeekUtc = TimeZoneInfo.ConvertTimeToUtc(endOfLastWeekVN, vnTimeZone);

            // Đếm số lượng apartment tạo ra trong từng tuần
            var apartmentLastWeek = await dbContext.Apartments
                .CountAsync(a => a.User.CreateAt >= startOfLastWeekUtc
                              && a.User.CreateAt < endOfLastWeekUtc);

            var apartmentThisWeek = await dbContext.Apartments
                .CountAsync(a => a.User.CreateAt >= startOfThisWeekUtc
                              && a.User.CreateAt < endOfThisWeekUtc);

            // Tính tăng/giảm
            int delta = apartmentThisWeek - apartmentLastWeek;
            double growthRate;
            if (apartmentLastWeek > 0)
                growthRate = Math.Round(delta * 100.0 / apartmentLastWeek, 1);
            else
                growthRate = apartmentThisWeek > 0 ? 100.0 : 0.0;

            var trend = growthRate >= 0
                ? $"Increased compared to last week"
                : $"Decreased compared to last week";
            var totalApartment = await dbContext.Apartments
                .CountAsync();
            var ownerApartment = await dbContext.Apartments
                .Where(a => a.UserId != null)
                .CountAsync();
            // Build DTO
            return new GenerateDashboardDTO
            {
                Total = Math.Round((double)ownerApartment / totalApartment * 100, 1),
                Increase = delta,
                GrowthRatePercent = growthRate,
                TrendDescription = trend,
                ApartmentThisWeek = Math.Round((double)apartmentThisWeek / totalApartment * 100, 1),
                ApartmentLastWeek = Math.Round((double) apartmentLastWeek /totalApartment * 100, 1)
            };
        }

    }
}

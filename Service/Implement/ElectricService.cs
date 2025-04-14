using ClosedXML.Excel;
using EzCondo_Data.Context;
using EzConDo_Service.DTO;
using EzConDo_Service.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EzConDo_Service.ExceptionsConfig.CustomException;

namespace EzConDo_Service.Implement
{
    public class ElectricService : IElectricService
    {
        private readonly ApartmentDbContext dbContext;

        public ElectricService(ApartmentDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task<List<ElectricMetterDTO>> AddElectricMettersAsync(IFormFile file)
        {
            if(file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty or null");
            }

            var electricMetters = new List<ElectricMetterDTO>();

            using (var stream = file.OpenReadStream())
            using (var workbook = new XLWorkbook(stream))
            {
                var worksheet = workbook.Worksheet(1);
                var rows = worksheet.RowsUsed().Skip(1);
                foreach (var row in rows)
                {
                    //id
                    string idValue = row.Cell(1).GetValue<string>().Trim();
                    Guid? id = null;
                    if (Guid.TryParse(idValue, out Guid guidValue))
                    {
                        id = guidValue;
                    }

                    //MeterNumber
                    string meterNumber = row.Cell(2).GetValue<string>().Trim();

                    //InstallationDate
                    DateOnly installationDate;
                    var cell = row.Cell(3);

                    if (cell.DataType == XLDataType.DateTime)
                    {
                        installationDate = DateOnly.FromDateTime(cell.GetDateTime());
                    }
                    else if (cell.DataType == XLDataType.Number)
                    {
                        installationDate = DateOnly.FromDateTime(DateTime.FromOADate(cell.GetDouble()));
                    }
                    else
                    {
                        string installationDateValue = cell.GetValue<string>().Trim();
                        var validFormats = new[] { "yyyy-MM-dd", "M/d/yyyy", "MM/dd/yyyy", "d/M/yyyy", "dd/MM/yyyy" };

                        if (!DateOnly.TryParseExact(installationDateValue, validFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out installationDate))
                        {
                            throw new ConflictException($"Invalid installation date at the line: {row.RowNumber()} - Value: {installationDateValue}");
                        }
                    }

                    //ApartmentNumber
                    string apartmentNumber = row.Cell(4).GetValue<string>().Trim();
                    var apartment = await dbContext.Apartments.FirstOrDefaultAsync(a => a.ApartmentNumber == apartmentNumber) 
                        ?? throw new NotFoundException($"Apartment '{apartmentNumber}' at row: {row.RowNumber()} is not found"); 


                    var dto = new ElectricMetterDTO
                    {
                        Id = id,
                        MeterNumber = meterNumber,
                        InstallationDate = installationDate,
                        ApartmentId = apartment.Id
                    };

                    // Check exist
                    var existingMeter = await dbContext.ElectricMeters
                        .FirstOrDefaultAsync(em => em.MeterNumber == dto.MeterNumber);
                    if (existingMeter != null)
                    {
                        // Ví dụ: nếu tồn tại bỏ qua và tiếp tục đọc dòng tiếp theo.
                        continue;
                    }

                    electricMetters.Add(dto);
                }
                //map dto -> domain
                var entities = electricMetters.Select(dto => new ElectricMeter
                {
                    Id = Guid.NewGuid(),
                    MeterNumber = dto.MeterNumber,
                    InstallationDate = dto.InstallationDate,
                    ApartmentId = dto.ApartmentId
                });

                await dbContext.AddRangeAsync(entities);
                await dbContext.SaveChangesAsync();

                return electricMetters;
            }
        }

        public async Task<List<ElectricReadingDTO>> AddElectricReadingAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty or null");

            var electricReadings = new List<ElectricReadingDTO>();

            using (var stream = file.OpenReadStream())
            using (var workbook = new XLWorkbook(stream))
            {
                var worksheet = workbook.Worksheet(1);
                var rows = worksheet.RowsUsed().Skip(1);

                foreach (var row in rows)
                {
                    string meterNumber = row.Cell(2).GetValue<string>().Trim();
                    string currentElectricStr = row.Cell(3).GetValue<string>().Trim();

                    if (string.IsNullOrEmpty(meterNumber) || string.IsNullOrEmpty(currentElectricStr))
                        throw new Exception($"Missing data at row {row.RowNumber()}");

                    var electricMeter = await dbContext.ElectricMeters
                        .FirstOrDefaultAsync(e => e.MeterNumber == meterNumber)
                        ?? throw new NotFoundException($"Electric meter '{meterNumber}' at row {row.RowNumber()} not found");

                    //Check apartment use by user
                    var apartment = await dbContext.Apartments.
                        FirstOrDefaultAsync(a => a.Id == electricMeter.ApartmentId && a.UserId != null)
                        ?? throw new ConflictException($"Apartment id: {electricMeter.ApartmentId} have no users!");


                    if (!decimal.TryParse(currentElectricStr, out var currentElectricNumber))
                        throw new Exception($"Invalid number format at row {row.RowNumber()}");

                    // Tìm lần đọc gần nhất để lấy số điện cũ
                    var lastReading = await dbContext.ElectricReadings
                        .Where(r => r.ElectricMetersId == electricMeter.Id)
                        .OrderByDescending(r => r.ReadingDate)
                        .FirstOrDefaultAsync();

                    decimal preElectricNumber = lastReading?.CurrentElectricNumber ?? 0; // Nếu không có lần đọc nào trước đó, gán = 0
                    decimal consumption = currentElectricNumber - preElectricNumber;

                    if (consumption < 0)
                        throw new Exception($"Invalid reading: new reading ({currentElectricNumber}) < previous reading ({preElectricNumber}) at row {row.RowNumber()}");

                    var dto = new ElectricReadingDTO
                    {
                        Id = Guid.NewGuid(),
                        ElectricMetersId = electricMeter.Id,
                        PreElectricNumber = preElectricNumber,
                        CurrentElectricNumber = currentElectricNumber,
                        Consumption = consumption,
                        ReadingDate = DateTime.Now
                    };

                    electricReadings.Add(dto);
                }

                // Map DTO -> Entity
                var entities = electricReadings.Select(dto => new ElectricReading
                {
                    Id = dto.Id,
                    ElectricMetersId = dto.ElectricMetersId,
                    ReadingDate = dto.ReadingDate,
                    PreElectricNumber = dto.PreElectricNumber,
                    CurrentElectricNumber = dto.CurrentElectricNumber,
                    Consumption = dto.Consumption
                });

                await dbContext.AddRangeAsync(entities);
                await dbContext.SaveChangesAsync();

                //add ElectricBill
                foreach (var reading in electricReadings)
                {
                    await AddElectricBillAsync(reading.Id); // bỏ qua chờ đợi 
                }

                return electricReadings;
            }
        }

        public async Task<List<ElectricMetterDTO>> GetAllElectricMettersAsync()
        {
            var electricMetters = await dbContext.ElectricMeters
                .Include(em => em.Apartment)
                .Select(em => new ElectricMetterDTO
                {
                    Id = em.Id,
                    MeterNumber = em.MeterNumber,
                    InstallationDate = em.InstallationDate,
                    ApartmentId = em.ApartmentId
                })
                .ToListAsync();
            return electricMetters;
        }

        public async Task<List<ElectricReadingDTO>> GetAllElectricReadingsAsync()
        {
            var electricReadings = await dbContext.ElectricReadings
                .Include(er => er.ElectricMeters)
                .Select(er => new ElectricReadingDTO
                {
                    Id = er.Id,
                    ElectricMetersId = er.ElectricMetersId,
                    ReadingDate = er.ReadingDate,
                    PreElectricNumber = er.PreElectricNumber,
                    CurrentElectricNumber = er.CurrentElectricNumber,
                    Consumption = er.Consumption
                })
                .ToListAsync();
            return electricReadings;
        }

        public async Task<List<ElectricViewDTO>> GetAllElectricAsync(bool? status, int? day = 30)
        {
            var fromDate = DateTime.Now.AddDays(-day ?? 30);

            var query = from reading in dbContext.ElectricReadings
                        join bill in dbContext.ElectricBills
                            on reading.Id equals bill.ReadingId into billGroup
                        from electricBill in billGroup.DefaultIfEmpty() // LEFT JOIN
                        join meter in dbContext.ElectricMeters
                            on reading.ElectricMetersId equals meter.Id
                        join apartment in dbContext.Apartments
                            on meter.ApartmentId equals apartment.Id
                        join user in dbContext.Users
                            on apartment.UserId equals user.Id
                        where electricBill == null || electricBill.CreateDate >= fromDate
                        select new ElectricViewDTO
                        {
                            ElectricReadingId = reading.Id,
                            FullName = user.FullName,
                            PhoneNumber = user.PhoneNumber,
                            Email = user.Email,
                            ApartmentNumber = apartment.ApartmentNumber,
                            Consumption = reading.Consumption,
                            ReadingDate = reading.ReadingDate,
                            status = electricBill != null ? electricBill.Status : "null"
                        };

            if (status.HasValue)
            {
                if (status.Value)
                {
                    query = query.Where(x => x.status == "completed" || x.status == "pending");
                }
                else
                {
                    query = query.Where(x => x.status != "completed" && x.status != "pending");
                }
            }

            var result = await query.ToListAsync();
            return result;
        }

        public async Task<ElectricDetailDTO> GetElectricDetailAsync(Guid electricReadingId)
        {
            var electricReading = await dbContext.ElectricReadings
                                .Include(er => er.ElectricMeters)
                                .ThenInclude(em => em.Apartment)
                                .ThenInclude(a => a.User)
                                .FirstOrDefaultAsync(er => er.Id == electricReadingId)
                                ?? throw new NotFoundException($"ElectricReadingId {electricReadingId} is not found !");

            var tiers = await dbContext.PriceElectricTiers
                        .OrderBy(t => t.MinKWh)
                        .ToListAsync();

            decimal consumption = electricReading.Consumption;
            decimal remaining = consumption;
            decimal totalPrice = 0;

            foreach (var tier in tiers)
            {
                decimal tierMin = tier.MinKWh;
                decimal tierMax = tier.MaxKWh == 0 ? decimal.MaxValue : tier.MaxKWh;
                decimal unitsInTier = Math.Min(remaining, tierMax - tierMin + 1);

                if (consumption >= tierMin)
                {
                    totalPrice += unitsInTier * tier.PricePerKWh;
                    remaining -= unitsInTier;
                }

                if (remaining <= 0)
                    break;
            }

            return new ElectricDetailDTO
            {
                FullName = electricReading.ElectricMeters.Apartment.User.FullName,
                PhoneNumber = electricReading.ElectricMeters.Apartment.User.PhoneNumber,
                Email = electricReading.ElectricMeters.Apartment.User.Email,
                ApartmentNumber = electricReading.ElectricMeters.Apartment.ApartmentNumber,
                MeterNumber = electricReading.ElectricMeters.MeterNumber,
                consumption = electricReading.Consumption,
                readingDate = electricReading.ReadingDate,
                pre_electric_number = electricReading.PreElectricNumber,
                current_electric_number = electricReading.CurrentElectricNumber,
                price = totalPrice
            };
        }

        public async Task<Guid?> AddElectricBillAsync(Guid electricReadingId)
        {
            var electricReading = await dbContext.ElectricReadings
                               .Include(er => er.ElectricMeters)
                               .ThenInclude(em => em.Apartment)
                               .ThenInclude(a => a.User)
                               .FirstOrDefaultAsync(er => er.Id == electricReadingId)
                               ?? throw new NotFoundException($"ElectricReadingId {electricReadingId} is not found !");

            var tiers = await dbContext.PriceElectricTiers
                        .OrderBy(t => t.MinKWh)
                        .ToListAsync();

            decimal consumption = electricReading.Consumption;
            decimal remaining = consumption;
            decimal totalPrice = 0;

            foreach (var tier in tiers)
            {
                decimal tierMin = tier.MinKWh;
                decimal tierMax = tier.MaxKWh == 0 ? decimal.MaxValue : tier.MaxKWh;
                decimal unitsInTier = Math.Min(remaining, tierMax - tierMin + 1);

                if (consumption >= tierMin)
                {
                    totalPrice += unitsInTier * tier.PricePerKWh;
                    remaining -= unitsInTier;
                }

                if (remaining <= 0)
                    break;
            }

            await dbContext.AddAsync(new ElectricBill
            {
                Id = Guid.NewGuid(),
                CustomerId = (Guid)electricReading.ElectricMeters.Apartment.UserId,
                ReadingId = electricReading.Id,
                TotalComsumption = electricReading.Consumption,
                TotalAmount = totalPrice,
                CreateDate = DateTime.Now,
                Status = "pending"
            });
            await dbContext.SaveChangesAsync();
            return electricReading.ElectricMeters.Apartment.UserId;
        }

        public async Task<List<MyElectricDetailDTO>> GetMyElectricDetailAsync(Guid userId, bool? status)
        {
            var query = from reading in dbContext.ElectricReadings
                        join bill in dbContext.ElectricBills
                            on reading.Id equals bill.ReadingId 
                        join meter in dbContext.ElectricMeters
                            on reading.ElectricMetersId equals meter.Id
                        join apartment in dbContext.Apartments
                            on meter.ApartmentId equals apartment.Id
                        join user in dbContext.Users
                            on apartment.UserId equals user.Id
                        where bill.CustomerId == userId
                        select new MyElectricDetailDTO
                        {
                            FullName = user.FullName,
                            PhoneNumber = user.PhoneNumber,
                            Email = user.Email,
                            ApartmentNumber = apartment.ApartmentNumber,
                            MeterNumber = meter.MeterNumber,
                            consumption = reading.Consumption,
                            pre_electric_number = reading.PreElectricNumber,
                            current_electric_number = reading.CurrentElectricNumber,
                            readingDate = reading.ReadingDate,
                            price = bill.TotalAmount,
                            status = bill.Status
                        };
            if (status.HasValue)
            {
                if (status.Value)
                {
                    query = query.Where(x => x.status == "completed");
                }
                else
                {
                    query = query.Where(x => x.status != "completed");
                }
            }

            var result = await query.ToListAsync();
            return result;
        }
    }
}

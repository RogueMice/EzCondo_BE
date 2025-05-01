using ClosedXML.Excel;
using EzCondo_Data.Context;
using EzCondo_Data.Domain;
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
            if (file == null || file.Length == 0)
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
                    Guid? id = Guid.NewGuid();

                    //MeterNumber
                    string meterNumber = row.Cell(1).GetValue<string>().Trim();

                    //InstallationDate
                    DateOnly installationDate;
                    var cell = row.Cell(2);

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
                    string apartmentNumber = row.Cell(3).GetValue<string>().Trim();
                    var apartment = await dbContext.Apartments.FirstOrDefaultAsync(a => a.ApartmentNumber == apartmentNumber)
                        ?? throw new NotFoundException($"Apartment '{apartmentNumber}' at row: {row.RowNumber()} is not found");


                    var dto = new ElectricMetterDTO
                    {
                        Id = id,
                        MeterNumber = meterNumber,
                        InstallationDate = installationDate,
                        ApartmentId = apartment.Id,
                        ApartmentNumber = apartment.ApartmentNumber
                    };

                    // Check exist
                    var existingMeterOrAparmentId = await dbContext.ElectricMeters
                        .FirstOrDefaultAsync(em => em.MeterNumber == dto.MeterNumber || em.ApartmentId == dto.ApartmentId);
                    if (existingMeterOrAparmentId != null)
                    {
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
                    string meterNumber = row.Cell(1).GetValue<string>().Trim();
                    string currentElectricStr = row.Cell(2).GetValue<string>().Trim();

                    if (string.IsNullOrEmpty(meterNumber) || string.IsNullOrEmpty(currentElectricStr))
                        throw new Exception($"Missing data at row {row.RowNumber()}");

                    var electricMeter = await dbContext.ElectricMeters
                        .FirstOrDefaultAsync(e => e.MeterNumber == meterNumber)
                        ?? throw new NotFoundException($"Electric meter '{meterNumber}' at row {row.RowNumber()} not found");

                    //Check apartment use by user
                    var apartment = await dbContext.Apartments.
                        FirstOrDefaultAsync(a => a.Id == electricMeter.ApartmentId && a.UserId != null)
                        ?? throw new ConflictException($"Apartment of Metter number: {electricMeter.MeterNumber} have no users!");

                    if (!decimal.TryParse(currentElectricStr, out var currentElectricNumber))
                        throw new Exception($"Invalid number format at row {row.RowNumber()}");

                    // Tìm lần đọc gần nhất để lấy số điện cũ
                    var lastReading = await dbContext.ElectricReadings
                        .Where(r => r.ElectricMetersId == electricMeter.Id)
                        .OrderByDescending(r => r.ReadingCurrentDate)
                        .FirstOrDefaultAsync();

                    // Define current reading date
                    var readingDate = DateTime.UtcNow;
                    // Set pre-date to the previous reading's date (or default if none)
                    var preDate = lastReading != null
                        ? lastReading.ReadingCurrentDate
                        : DateTime.UtcNow;

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
                        ReadingPreDate = preDate,
                        ReadingCurrentDate = readingDate
                    };

                    electricReadings.Add(dto);
                }

                // Map DTO -> Entity
                var entities = electricReadings.Select(dto => new ElectricReading
                {
                    Id = dto.Id,
                    ElectricMetersId = dto.ElectricMetersId,
                    ReadingPreDate = dto.ReadingPreDate,
                    ReadingCurrentDate = dto.ReadingCurrentDate,
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
                    ApartmentId = em.ApartmentId,
                    ApartmentNumber = em.Apartment.ApartmentNumber // Added to return apartment number
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
                    ReadingPreDate = er.ReadingPreDate,
                    ReadingCurrentDate = er.ReadingCurrentDate,
                    PreElectricNumber = er.PreElectricNumber,
                    CurrentElectricNumber = er.CurrentElectricNumber,
                    Consumption = er.Consumption
                })
                .ToListAsync();
            return electricReadings;
        }

        public async Task<List<ElectricViewDTO>> GetAllElectricAsync(bool? status, int? day)
        {
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
                        select new
                        {
                            Reading = reading,
                            Bill = electricBill,
                            User = user,
                            Apartment = apartment
                        };

            if (day.HasValue)
            {
                var fromDate = DateTime.Now.AddDays(-day.Value);
                query = query.Where(x => x.Bill == null || x.Bill.CreateDate >= fromDate);
            }

            // Chuyển về DTO
            var dtoQuery = query.Select(x => new ElectricViewDTO
            {
                ElectricReadingId = x.Reading.Id,
                FullName = x.User.FullName,
                PhoneNumber = x.User.PhoneNumber,
                Email = x.User.Email,
                ApartmentNumber = x.Apartment.ApartmentNumber,
                Consumption = x.Reading.Consumption,
                ReadingPreDate = x.Reading.ReadingPreDate,
                ReadingCurrentDate = x.Reading.ReadingCurrentDate,
                status = x.Bill != null ? x.Bill.Status : "null"
            });

            if (status.HasValue)
            {
                if (status.Value)
                {
                    dtoQuery = dtoQuery.Where(x => x.status == "completed");
                }
                else
                {
                    dtoQuery = dtoQuery.Where(x => x.status != "completed");
                }
            }

            return await dtoQuery.ToListAsync();
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
                Consumption = electricReading.Consumption,
                ReadingPreDate = electricReading.ReadingPreDate,
                ReadingCurrentDate = electricReading.ReadingCurrentDate,
                Pre_electric_number = electricReading.PreElectricNumber,
                Current_electric_number = electricReading.CurrentElectricNumber,
                Price = totalPrice
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

            //Lấy bản ghi điện ngay trước đó của cùng 1 công tơ
            var previous = await dbContext.ElectricReadings
                                          .Where(er =>
                                                er.ElectricMetersId == electricReading.ElectricMetersId
                                                && er.ReadingCurrentDate < electricReading.ReadingCurrentDate)
                                          .OrderByDescending(er => er.ReadingCurrentDate)
                                          .FirstOrDefaultAsync();

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
                CreateDate = DateTime.UtcNow,
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
                            readingDate = reading.ReadingCurrentDate,  
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

        public async Task<byte[]> CreateTemplateElectricMetterAsync()
        {
            //Lấy danh sách ApartmentNumber: loại trừ role Manager và các phòng đã có người sử dụng
            var apartmentNumbers = await dbContext.Apartments
                .Where(a => a.UserId != null
                       && a.User.Role.Name.ToLower() == "resident"
                       && a.ElectricMeter == null)
                .Select(a => a.ApartmentNumber)
                .ToListAsync();
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Template");

            // Header row
            worksheet.Cell(1, 1).Value = "MeterNumber";
            worksheet.Cell(1, 2).Value = "InstallationDate (yyyy-MM-dd)";
            worksheet.Cell(1, 3).Value = "ApartmentNumber";

            // Format header
            var headerRange = worksheet.Range(1, 1, 1, 3);
            headerRange.Style.Font.Bold = true;
            worksheet.SheetView.FreezeRows(1);

            //Đổ danh sách ApartmentNumber vào cột 3, từ row 2
            int row = 2;
            foreach (var apt in apartmentNumbers)
            {
                worksheet.Cell(row, 3).Value = apt;
                row++;
            }

            worksheet.Columns(1, 3).AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> CreateTemplateElectricReadingAsync()
        {
            var metterNumber = await dbContext.ElectricMeters
                .Select(em => em.MeterNumber)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Template");

            // Header row
            worksheet.Cell(1, 1).Value = "Metter Number";
            worksheet.Cell(1, 2).Value = "Current Electric Number";

            // Format header
            var headerRange = worksheet.Range(1, 1, 1, 2);
            headerRange.Style.Font.Bold = true;
            worksheet.SheetView.FreezeRows(1);

            //show data
            int row = 2;
            foreach (var meter in metterNumber)
            {
                worksheet.Cell(row, 1).Value = meter;
                row++;
            }

            // Optionally set column widths
            worksheet.Columns(1, 2).AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<string> UpdateElectricBillsAsync(List<UpdateElectricBillDTO> dtos)
        {
            if (dtos == null || dtos.Count == 0)
                throw new BadRequestException("Danh sách DTO không được để trống.");

            var ids = dtos.Select(d => d.ElectricBillId).ToList();
            var bills = await dbContext.ElectricBills
                                        .Where(b => ids.Contains(b.Id))
                                        .ToListAsync();

            var missingIds = ids.Except(bills.Select(b => b.Id)).ToList();
            if (missingIds.Any())
            {
                throw new NotFoundException($"Không tìm thấy một số ElectricBill sau: {missingIds}");
            }

            foreach (var dto in dtos)
            {
                var bill = bills.First(b => b.Id == dto.ElectricBillId);
                bill.Status = "overdue";
            }
            await dbContext.SaveChangesAsync();
            return "Update success !";
        }
    }
}

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
    public class WaterService: IWaterService
    {
        private readonly ApartmentDbContext dbContext;

        public WaterService(ApartmentDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public Task<byte[]> CreateTemplateWaterMetterAsync()
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Template");

            // Header row
            worksheet.Cell(1, 1).Value = "Meter Number";
            worksheet.Cell(1, 2).Value = "InstallationDate (yyyy-MM-dd)";
            worksheet.Cell(1, 3).Value = "Apartment Number";

            // Format header
            var headerRange = worksheet.Range(1, 1, 1, 3);
            headerRange.Style.Font.Bold = true;
            worksheet.SheetView.FreezeRows(1);

            // Optionally set column widths
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return Task.FromResult(stream.ToArray());
        }

        public Task<byte[]> CreateTemplateWaterReadingAsync()
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Template");

            // Header row
            worksheet.Cell(1, 1).Value = "Metter Number";
            worksheet.Cell(1, 2).Value = "Current Water Number";

            // Format header
            var headerRange = worksheet.Range(1, 1, 1, 2);
            headerRange.Style.Font.Bold = true;
            worksheet.SheetView.FreezeRows(1);

            // Optionally set column widths
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return Task.FromResult(stream.ToArray());
        }

        public async Task<List<WaterMetterDTO>> AddWaterMettersAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty or null");
            }

            var waterMetters = new List<WaterMetterDTO>();

            using (var stream = file.OpenReadStream())
            using (var workbook = new XLWorkbook(stream))
            {
                var worksheet = workbook.Worksheet(1);
                var rows = worksheet.RowsUsed().Skip(1);
                foreach (var row in rows)
                {
                    //id
                    Guid id = Guid.NewGuid();

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


                    var dto = new WaterMetterDTO
                    {
                        Id = Guid.NewGuid(),
                        MeterNumber = meterNumber,
                        InstallationDate = installationDate,
                        ApartmentId = apartment.Id,
                        ApartmentNumber = apartment.ApartmentNumber
                    };

                    // Check exist
                    var existingMeterOrAparmentId = await dbContext.WaterMeters
                        .FirstOrDefaultAsync(em => em.MeterNumber == dto.MeterNumber || em.ApartmentId == dto.ApartmentId);
                    if (existingMeterOrAparmentId != null)
                    {
                        continue;
                    }

                    waterMetters.Add(dto);
                }
                //map dto -> domain
                var entities = waterMetters.Select(dto => new WaterMeter
                {
                    Id = Guid.NewGuid(),
                    MeterNumber = dto.MeterNumber,
                    InstallationDate = dto.InstallationDate,
                    ApartmentId = dto.ApartmentId
                });

                await dbContext.AddRangeAsync(entities);
                await dbContext.SaveChangesAsync();

                return waterMetters;
            }
        }

        public async Task<List<WaterReadingDTO>> AddWaterReadingAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty or null");

            var waterReading = new List<WaterReadingDTO>();

            using (var stream = file.OpenReadStream())
            using (var workbook = new XLWorkbook(stream))
            {
                var worksheet = workbook.Worksheet(1);
                var rows = worksheet.RowsUsed().Skip(1);

                foreach (var row in rows)
                {
                    string meterNumber = row.Cell(1).GetValue<string>().Trim();
                    string currentWater = row.Cell(2).GetValue<string>().Trim();

                    if (string.IsNullOrEmpty(meterNumber) || string.IsNullOrEmpty(currentWater))
                        throw new Exception($"Missing data at row {row.RowNumber()}");

                    var waterMetter = await dbContext.WaterMeters
                        .FirstOrDefaultAsync(w => w.MeterNumber == meterNumber)
                        ?? throw new NotFoundException($"Electric meter '{meterNumber}' at row {row.RowNumber()} not found");

                    //Check apartment use by user
                    var apartment = await dbContext.Apartments.
                        FirstOrDefaultAsync(a => a.Id == waterMetter.ApartmentId && a.UserId != null)
                        ?? throw new ConflictException($"Apartment of Metter number: {waterMetter.MeterNumber} have no users!");

                    if (!decimal.TryParse(currentWater, out var currentWaterNumber))
                        throw new Exception($"Invalid number format at row {row.RowNumber()}");

                    // Tìm lần đọc gần nhất để lấy số nước cũ
                    var lastReading = await dbContext.WaterReadings
                        .Where(r => r.WaterMetersId == waterMetter.Id)
                        .OrderByDescending(r => r.ReadingDate)
                        .FirstOrDefaultAsync();

                    decimal preWaterNumber = lastReading?.CurrentWaterNumber ?? 0; // Nếu không có lần đọc nào trước đó, gán = 0
                    decimal consumption = currentWaterNumber - preWaterNumber;

                    if (consumption < 0)
                        throw new Exception($"Invalid reading: new reading ({currentWaterNumber}) < previous reading ({preWaterNumber}) at row {row.RowNumber()}");

                    var dto = new WaterReadingDTO
                    {
                        Id = Guid.NewGuid(),
                        WaterMetersId = waterMetter.Id,
                        PreWaterNumber = preWaterNumber,
                        CurrentWaterNumber = currentWaterNumber,
                        Consumption = consumption,
                        ReadingDate = DateTime.Now
                    };

                    waterReading.Add(dto);
                }

                // Map DTO -> Entity
                var entities = waterReading.Select(dto => new WaterReading
                {
                    Id = dto.Id,
                    WaterMetersId = dto.WaterMetersId,
                    ReadingDate = dto.ReadingDate,
                    PreWaterNumber = dto.PreWaterNumber,
                    CurrentWaterNumber = dto.CurrentWaterNumber,
                    Consumption = dto.Consumption
                });

                await dbContext.AddRangeAsync(entities);
                await dbContext.SaveChangesAsync();

                //add ElectricBill
                foreach (var reading in waterReading)
                {
                    await AddWaterBillAsync(reading.Id); // bỏ qua chờ đợi 
                }

                return waterReading;
            }
        }

        public async Task<Guid?> AddWaterBillAsync(Guid waterReadingId)
        {
            var waterReading = await dbContext.WaterReadings
                   .Include(w => w.WaterMeters)
                   .ThenInclude(em => em.Apartment)
                   .ThenInclude(a => a.User)
                   .FirstOrDefaultAsync(er => er.Id == waterReadingId)
                   ?? throw new NotFoundException($"ElectricReadingId {waterReadingId} is not found !");

            var tiers = await dbContext.PriceWaterTiers.ToListAsync();

            decimal consumption = waterReading.Consumption;
            decimal totalPrice = 0;
            foreach (var tier in tiers)
            {
                totalPrice += tier.PricePerM3 * consumption;
            }

            await dbContext.AddAsync(new WaterBill
            {
                Id = Guid.NewGuid(),
                CustomerId = (Guid)waterReading.WaterMeters.Apartment.UserId,
                ReadingId = waterReading.Id,
                TotalConsumption = waterReading.Consumption,
                TotalAmount = totalPrice,
                CreateDate = DateTime.Now,
                Status = "pending"
            });
            await dbContext.SaveChangesAsync();
            return waterReading.WaterMeters.Apartment.UserId;
        }

        public async Task<List<WaterMetterDTO>> GetAllWaterMettersAsync()
        {
            var waterMetters = await dbContext.WaterMeters
                .Include(em => em.Apartment)
                .Select(em => new WaterMetterDTO
                {
                    Id = em.Id,
                    MeterNumber = em.MeterNumber,
                    InstallationDate = em.InstallationDate,
                    ApartmentId = em.ApartmentId,
                    ApartmentNumber = em.Apartment.ApartmentNumber // Added to return apartment number
                })
                .ToListAsync();
            return waterMetters;
        }

        public async Task<List<WaterReadingDTO>> GetAllWaterReadingsAsync()
        {
            var waterReadings = await dbContext.WaterReadings
                .Include(er => er.WaterMeters)
                .Select(er => new WaterReadingDTO
                {
                    Id = er.Id,
                    WaterMetersId = er.WaterMetersId,
                    ReadingDate = er.ReadingDate,
                    PreWaterNumber = er.PreWaterNumber,
                    CurrentWaterNumber = er.CurrentWaterNumber,
                    Consumption = er.Consumption
                }).ToListAsync();
            return waterReadings;
        }

        public async Task<List<WaterViewDTO>> GetAllWaterAsync(bool? status, int? day = 30)
        {
            var fromDate = DateTime.Now.AddDays(-day ?? 30);

            var query = from reading in dbContext.WaterReadings
                        join bill in dbContext.WaterBills
                            on reading.Id equals bill.ReadingId into billGroup
                        from waterBill in billGroup.DefaultIfEmpty() // LEFT JOIN
                        join meter in dbContext.WaterMeters
                            on reading.WaterMetersId equals meter.Id
                        join apartment in dbContext.Apartments
                            on meter.ApartmentId equals apartment.Id
                        join user in dbContext.Users
                            on apartment.UserId equals user.Id
                        where waterBill == null || waterBill.CreateDate >= fromDate
                        select new WaterViewDTO
                        {
                            Id = reading.Id,
                            FullName = user.FullName,
                            PhoneNumber = user.PhoneNumber,
                            Email = user.Email,
                            ApartmentNumber = apartment.ApartmentNumber,
                            Consumption = reading.Consumption,
                            ReadingDate = reading.ReadingDate,
                            status = waterBill != null ? waterBill.Status : "null"
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

        public async Task<WaterDetailDTO> GetWaterDetailAsync(Guid waterReadingId)
        {
            var waterReading = await dbContext.WaterReadings
                                .Include(w => w.WaterMeters)
                                .ThenInclude(em => em.Apartment)
                                .ThenInclude(a => a.User)
                                .FirstOrDefaultAsync(w => w.Id == waterReadingId)
                                ?? throw new NotFoundException($"WaterReadingId {waterReadingId} is not found !");

            var tiers = await dbContext.PriceWaterTiers.ToListAsync();

            decimal consumption = waterReading.Consumption;
            decimal totalPrice = 0;
            foreach (var tier in tiers)
            {
                totalPrice += tier.PricePerM3 * consumption;
            }

            return new WaterDetailDTO
            {
                FullName = waterReading.WaterMeters.Apartment.User.FullName,
                PhoneNumber = waterReading.WaterMeters.Apartment.User.PhoneNumber,
                Email = waterReading.WaterMeters.Apartment.User.Email,
                ApartmentNumber = waterReading.WaterMeters.Apartment.ApartmentNumber,
                MeterNumber = waterReading.WaterMeters.MeterNumber,
                consumption = waterReading.Consumption,
                readingDate = waterReading.ReadingDate,
                pre_water_number = waterReading.PreWaterNumber,
                current_water_number = waterReading.CurrentWaterNumber,
                price = totalPrice
            };
        }

        public async Task<List<MyWaterDetailDTO>> GetMyWaterDetailAsync(Guid userId, bool? status)
        {
            var query = from reading in dbContext.WaterReadings
                        join bill in dbContext.WaterBills
                            on reading.Id equals bill.ReadingId
                        join meter in dbContext.WaterMeters
                            on reading.WaterMetersId equals meter.Id
                        join apartment in dbContext.Apartments
                            on meter.ApartmentId equals apartment.Id
                        join user in dbContext.Users
                            on apartment.UserId equals user.Id
                        where bill.CustomerId == userId
                        select new MyWaterDetailDTO
                        {
                            FullName = user.FullName,
                            PhoneNumber = user.PhoneNumber,
                            Email = user.Email,
                            ApartmentNumber = apartment.ApartmentNumber,
                            MeterNumber = meter.MeterNumber,
                            consumption = reading.Consumption,
                            pre_water_number = reading.PreWaterNumber,
                            current_water_number = reading.CurrentWaterNumber,
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

﻿using ClosedXML.Excel;
using EzCondo_Data.Context;
using EzCondo_Data.Domain;
using EzConDo_Service.DTO;
using EzConDo_Service.FirebaseIntegration;
using EzConDo_Service.Interface;
using FirebaseAdmin.Auth;
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
        private readonly IFirebasePushNotificationService firebasePush;

        public WaterService(ApartmentDbContext dbContext, IFirebasePushNotificationService firebasePush)
        {
            this.dbContext = dbContext;
            this.firebasePush = firebasePush;
        }

        public async Task<byte[]> CreateTemplateWaterMetterAsync()
        {
            // Lấy danh sách ApartmentNumber: loại trừ role Manager và các phòng đã có người sử dụng
            var apartmentNumbers = await dbContext.Apartments
                .Where(a => a.UserId != null 
                       && a.User.Role.Name.ToLower() == "resident"
                       && !a.WaterMeters.Any())
                .Select(a => a.ApartmentNumber)
                .ToListAsync();

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

        public async Task<byte[]> CreateTemplateWaterReadingAsync()
        {
            var metterNumber = await dbContext.WaterMeters
                .Select(em => em.MeterNumber)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Template");

            // Header row
            worksheet.Cell(1, 1).Value = "Metter Number";
            worksheet.Cell(1, 2).Value = "Current Water Number";

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
                        .OrderByDescending(r => r.ReadingCurrentDate)
                        .FirstOrDefaultAsync();

                    // Define current reading date
                    var readingDate = DateTime.UtcNow;
                    // Set pre-date to the previous reading's date (or default if none)
                    var preDate = lastReading != null
                        ? lastReading.ReadingCurrentDate
                        : DateTime.UtcNow;

                    decimal preWaterNumber = lastReading?.CurrentWaterNumber ?? 0; // Nếu không có lần đọc nào trước đó, gán = 0
                    decimal consumption = currentWaterNumber - preWaterNumber;

                    if (consumption < 0)
                        throw new Exception($"Invalid reading: new reading ({currentWaterNumber}) < previous reading ({preWaterNumber}) at row {row.RowNumber()}");

                    var dto = new WaterReadingDTO
                    {
                        Id = Guid.NewGuid(),
                        WaterMetersId = waterMetter.Id,
                        ReadingPreDate = preDate,
                        PreWaterNumber = preWaterNumber,
                        CurrentWaterNumber = currentWaterNumber,
                        Consumption = consumption,
                        ReadingCurrentDate = readingDate
                    };

                    waterReading.Add(dto);
                }

                // Map DTO -> Entity
                var entities = waterReading.Select(dto => new WaterReading
                {
                    Id = dto.Id,
                    WaterMetersId = dto.WaterMetersId,
                    ReadingPreDate = dto.ReadingPreDate,
                    ReadingCurrentDate = dto.ReadingCurrentDate,
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
                    ReadingPreDate = (DateTime)er.ReadingPreDate,
                    ReadingCurrentDate = er.ReadingCurrentDate,
                    PreWaterNumber = er.PreWaterNumber,
                    CurrentWaterNumber = er.CurrentWaterNumber,
                    Consumption = er.Consumption
                }).ToListAsync();
            return waterReadings;
        }

        public async Task<List<WaterViewDTO>> GetAllWaterAsync(bool? status, int? day, int? month)
        {
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
                        select new
                        {
                            Reading = reading,
                            Bill = waterBill,
                            Meter = meter,
                            Apartment = apartment,
                            User = user
                        };

            if (day.HasValue)
            {
                var fromDate = DateTime.UtcNow.AddDays(-day.Value);
                query = query.Where(x => x.Reading.ReadingCurrentDate <= fromDate);
            }
            if (month.HasValue)
            {
                query = query.Where(x =>
                    x.Bill != null &&
                    x.Bill.CreateDate.Month == month.Value
                );
            }

            var dtoQuery = query.Select(x => new WaterViewDTO
            {
                Id = x.Reading.Id,
                WaterBillId = x.Bill.Id,
                FullName = x.User.FullName,
                PhoneNumber = x.User.PhoneNumber,
                Email = x.User.Email,
                ApartmentNumber = x.Apartment.ApartmentNumber,
                Consumption = x.Reading.Consumption,
                ReadingPreDate = x.Reading.ReadingPreDate,
                ReadingCurrentDate = x.Reading.ReadingCurrentDate,
                Status = x.Bill != null ? x.Bill.Status : "null"
            });

            if (status.HasValue)
            {
                if (status.Value)
                {
                    dtoQuery = dtoQuery.Where(x => x.Status == "completed");
                }
                else
                {
                    dtoQuery = dtoQuery.Where(x => x.Status != "completed");
                }
            }

            return await dtoQuery.ToListAsync();
        }

        public async Task UpdateOverdueWaterBillsAsync()
        {
            var cutoff = DateTime.UtcNow.AddDays(-15);
            var billsToOverdue = await dbContext.WaterBills
                .Where(b => b.Status == "pending" && b.CreateDate <= cutoff)
                .ToListAsync();

            if (billsToOverdue.Any())
            {
                foreach (var bill in billsToOverdue)
                {
                    bill.Status = "overdue";

                    var notification = new Notification
                    {
                        Id = Guid.NewGuid(),
                        Title = $"Quá hạn thanh toán tiền nước tháng {bill.CreateDate.Month}",
                        Content = $"Quý cư dân thân mến, hóa đơn tiền điện tháng {bill.CreateDate.Month} ({bill.TotalAmount:N0} VND) đã quá hạn thanh toán. " +
                                  "Vui lòng thực hiện thanh toán trong thời gian sớm nhất để đảm bảo quyền lợi và tránh các gián đoạn không mong muốn.",
                        Type = "Notice",
                        CreatedBy = bill.CustomerId,
                        CreatedAt = DateTime.UtcNow
                    };

                    dbContext.Notifications.Add(notification);

                    var receiver = new NotificationReceiver
                    {
                        Id = Guid.NewGuid(),
                        NotificationId = notification.Id,
                        UserId = bill.CustomerId,
                        Receiver = "resident",
                        IsRead = false,
                        ReadAt = null
                    };
                    dbContext.NotificationReceivers.Add(receiver);

                    var deviceTokens = await dbContext.UserDevices
                        .Where(ud => ud.UserId == bill.CustomerId && ud.IsActive)
                        .Select(ud => ud.FcmToken)
                        .ToListAsync();

                    if (deviceTokens.Any())
                    {
                        await firebasePush.SendPushNotificationAsync(
                            notification.Title,
                            notification.Content,
                            deviceTokens);
                    }
                }
                await dbContext.SaveChangesAsync();
            }
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
                Consumption = waterReading.Consumption,
                ReadingPreDate = waterReading.ReadingPreDate,
                ReadingCurrentDate = waterReading.ReadingCurrentDate,
                Pre_water_number = waterReading.PreWaterNumber,
                Current_water_number = waterReading.CurrentWaterNumber,
                Price = totalPrice
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
                            WaterBillId = bill.Id,
                            FullName = user.FullName,
                            PhoneNumber = user.PhoneNumber,
                            Email = user.Email,
                            ApartmentNumber = apartment.ApartmentNumber,
                            MeterNumber = meter.MeterNumber,
                            Consumption = reading.Consumption,
                            Pre_water_number = reading.PreWaterNumber,
                            Current_water_number = reading.CurrentWaterNumber,
                            ReadingPreDate = reading.ReadingPreDate,
                            ReadingCurrentDate = reading.ReadingCurrentDate,
                            Price = bill.TotalAmount,
                            Status = bill.Status,
                            PaymentTerm = bill.CreateDate.AddDays(15)
                        };
            if (status.HasValue)
            {
                if (status.Value)
                {
                    query = query.Where(x => x.Status == "completed");
                }
                else
                {
                    query = query.Where(x => x.Status != "completed");
                }
            }

            var result = await query.ToListAsync();
            return result;
        }

        public async Task<string> UpdateWaterBillsAsync(List<UpdateWaterBillDTO> dtos)
        {
            if (dtos == null || dtos.Count == 0)
                throw new BadRequestException("Danh sách DTO không được để trống.");

            var ids = dtos.Select(d => d.WaterBillId).ToList();
            var bills = await dbContext.WaterBills
                                        .Where(b => ids.Contains(b.Id))
                                        .ToListAsync();

            var missingIds = ids.Except(bills.Select(b => b.Id)).ToList();
            if (missingIds.Any())
            {
                throw new NotFoundException($"Không tìm thấy một số WaterBill sau: {missingIds}");
            }

            foreach (var dto in dtos)
            {
                var bill = bills.First(b => b.Id == dto.WaterBillId);
                bill.Status = "overdue";
            }
            await dbContext.SaveChangesAsync();
            return "Update success !";
        }
    }
}

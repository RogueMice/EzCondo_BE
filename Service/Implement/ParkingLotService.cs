using CloudinaryDotNet;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using EzCondo_Data.Context;
using EzCondo_Data.Domain;
using EzConDo_Service.DTO;
using EzConDo_Service.FirebaseIntegration;
using EzConDo_Service.Interface;
using FirebaseAdmin.Auth;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EzConDo_Service.ExceptionsConfig.CustomException;

namespace EzConDo_Service.Implement
{
    public class ParkingLotService : IParkingLotService
    {
        private readonly ApartmentDbContext dbContext;
        private readonly IFirebasePushNotificationService firebasePush;

        public ParkingLotService(ApartmentDbContext dbContext, IFirebasePushNotificationService firebasePush)
        {
            this.dbContext = dbContext;
            this.firebasePush = firebasePush;
        }
        public async Task<List<Guid>> AddParkingLotAsync(ParkingCardRequestDTO dtos, Guid userId)
        {
            if (dtos == null)
                throw new BadRequestException("Request body can't be empty!");

            int motors = dtos.NumberOfMotorbikes ?? 0;
            int cars = dtos.NumberOfCars ?? 0;
            int total = motors + cars;
            if (total == 0)
                throw new BadRequestException("You need to provide at least one card to create!");

            // 1. Lấy hoặc tạo mới ParkingLot
            var parkingLot = await dbContext.ParkingLots
                .FirstOrDefaultAsync(pl => pl.UserId == userId);

            if (parkingLot == null)
            {
                parkingLot = new ParkingLot
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Accept = false
                };
                await dbContext.ParkingLots.AddAsync(parkingLot);
            }

            // 2. Tạo danh sách ParkingLotDetail
            var details = new List<ParkingLotDetail>(total);
            for (int i = 0; i < motors; i++)
                details.Add(new ParkingLotDetail
                {
                    Id = Guid.NewGuid(),
                    ParkingLotId = parkingLot.Id,
                    Type = "motor",
                    Status = "pending",
                    Checking = false
                });
            for (int i = 0; i < cars; i++)
                details.Add(new ParkingLotDetail
                {
                    Id = Guid.NewGuid(),
                    ParkingLotId = parkingLot.Id,
                    Type = "car",
                    Status = "pending",
                    Checking = false
                });

            // 3. Thêm details với tắt AutoDetectChanges để tăng performance
            var tracker = dbContext.ChangeTracker;
            bool prevAutoDetect = tracker.AutoDetectChangesEnabled;
            try
            {
                tracker.AutoDetectChangesEnabled = false;
                await dbContext.ParkingLotDetails.AddRangeAsync(details);
                await dbContext.SaveChangesAsync();
            }
            finally
            {
                tracker.AutoDetectChangesEnabled = prevAutoDetect;
            }

            //update accept = false
            parkingLot.Accept = false;
            await dbContext.SaveChangesAsync();

            // 4. Trả về danh sách IDs của các thẻ vừa tạo
            return details.Select(d => d.Id).ToList();
        }

        public async Task<List<ParkingLotViewDTO>> GetAllParkingLotsAsync()
        {
            var flat = await dbContext.ParkingLotDetails
                .AsNoTracking()
                .Where(d => d.Status != "pending")
                .Select(d => new 
                {
                    ParkingId = d.ParkingLot.Id,
                    FullName = d.ParkingLot.User.FullName,
                    Apartment = d.ParkingLot.User.Apartments
                                        .OrderBy(a => a.Id)
                                        .Select(a => a.ApartmentNumber)
                                        .FirstOrDefault(),
                    Type = d.Type,
                    Accept = d.ParkingLot.Accept
                })
                .ToListAsync();

            var grouped = flat
                .GroupBy(x => x.ParkingId)
                .Select(g => new ParkingLotViewDTO
                {
                    ParkingId = g.Key,
                    Name = g.First().FullName,
                    Apartment = g.First().Apartment,
                    NumberOfMotorbike = g.Count(x => x.Type == "motor"),
                    NumberOfCar = g.Count(x => x.Type == "car"),
                    Accept = g.Select(x => x.Accept).FirstOrDefault()
                })
                .ToList();

            return grouped;
        }

        public async Task<string> UpdateOrDeleteAsync(ParkingLotAcceptOrRejectDTO dto)
        {
            var parkingLot = await dbContext.ParkingLots
               .Include(pl => pl.ParkingLotDetails)
               .SingleOrDefaultAsync(pl => pl.Id == dto.ParkingLotId)
               ?? throw new NotFoundException($"Not found parking lot {dto.ParkingLotId}.");

            var detailQuery = dbContext.ParkingLotDetails
                .Where(d => d.ParkingLot.Id == dto.ParkingLotId);

            var count = await detailQuery.CountAsync();
            if (count == 0)
                throw new NotFoundException($"Not found any card for parking lot {dto.ParkingLotId}.");
            if (dto.Accept)
            {
                //update parkingLot 
                parkingLot.Accept = dto.Accept;
                dbContext.ParkingLots.Update(parkingLot);

                // transaction
                await using var tx = await dbContext.Database.BeginTransactionAsync();

                //Get Price
                var price = await dbContext.PriceParkingLots
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
                if (price == null)
                    throw new InvalidOperationException("Price settings not found.");

                // total price
                var details = await detailQuery
                    .Where(d => d.Status != "active")
                    .Select(d => new { d.Type })
                    .ToListAsync();

                decimal totalAmount = details
                    .Sum(d => d.Type == "car"
                        ? (price.PricePerOto ?? throw new ConflictException("PricePerOto is null."))
                        : (price.PricePerMotor ?? throw new ConflictException("PricePerMotor is null.")));

                //Bulk-update status
                var updatedCount = await detailQuery.ExecuteUpdateAsync(s => s
                    .SetProperty(d => d.Status, _ => "active")
                );

                // get UserId
                var userId = await dbContext.ParkingLots
                    .Where(pl => pl.Id == dto.ParkingLotId)
                    .Select(pl => pl.UserId)
                    .FirstOrDefaultAsync()
                    ?? throw new ConflictException("UserId for this parking lot is null.");

                    //Tạo Payment
                    var invoice = new Payment
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        ParkingId = dto.ParkingLotId,
                        Amount = totalAmount,
                        Status = "pending",
                        Method = "VietQR",
                        CreateDate = DateTime.UtcNow
                    };
                    await dbContext.Payments.AddAsync(invoice);
                    await dbContext.SaveChangesAsync();

                //Cho chạy tự động, ngày đầu mỗi tháng tạo 1 hóa đơn mới dựa trên tất cả các thẻ của parkingId
                BackgroundJob.Schedule<IParkingLotService>(
                        svc => svc.CreateRecurringParkingBillAsync(invoice.Id),
                        TimeSpan.FromDays(30)
                    );
                //Send real-time to resident
                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    Title = $"Yêu cầu vé xe của bạn đã được phê duyệt",
                    Content = $"Chúng tôi xin thông báo rằng yêu cầu đăng ký vé xe của bạn tại bãi đỗ xe của chung cư đã được phê duyệt thành công. Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi.",
                    Type = "Notice",
                    CreatedBy = parkingLot.UserId.Value,
                    CreatedAt = DateTime.UtcNow
                };

                await dbContext.Notifications.AddAsync(notification);

                dbContext.NotificationReceivers.Add(new NotificationReceiver
                {
                    Id = Guid.NewGuid(),
                    NotificationId = notification.Id,
                    UserId = parkingLot.UserId.Value,
                    Receiver = "resident",
                    IsRead = false,
                    ReadAt = null
                });
                var deviceTokens = await dbContext.UserDevices
                    .Where(ud => ud.UserId == parkingLot.UserId && ud.IsActive)
                    .Select(ud => ud.FcmToken)
                    .ToListAsync();

                if (deviceTokens.Any())
                {
                    await firebasePush.SendPushNotificationAsync(
                        notification.Title,
                        notification.Content,
                        deviceTokens);
                }

                await dbContext.SaveChangesAsync();
                // Commit
                await tx.CommitAsync();

                return $"Accepted {updatedCount} card(s). Invoice {invoice.Id} created for {totalAmount:C}.";
            }
            else
            {
                await using var tx = await dbContext.Database.BeginTransactionAsync();
                var deletedCount = await detailQuery.ExecuteDeleteAsync();
                dbContext.ParkingLots.Remove(parkingLot);

                await tx.CommitAsync();
                return $"Reject success: deleted {deletedCount} card(s) and parking lot {dto.ParkingLotId}.";
            }
        }

        public async Task<List<ParkingLotDetailViewDTO>> GetParkingDetailAsync(Guid parkingLotId)
        {
            var price = await dbContext.PriceParkingLots
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (price == null)
                throw new ConflictException("Price Parking Lot is empting.");

            var details = await dbContext.ParkingLotDetails
                .AsNoTracking()
                .Where(d => d.ParkingLotId == parkingLotId)
                .Select(d => new ParkingLotDetailViewDTO
                {
                    Id = d.Id,
                    Checking = d.Checking ? "Check in" : "Check out",
                    Status = d.Status,
                    Type = d.Type,
                    Price = d.Type == "car"
                       ? price.PricePerOto ?? 0
                       : price.PricePerMotor ?? 0
                })
                .ToListAsync();

            return details;
        }

        public async Task<string> UpdateParkingLotDetailAsync(UpdateParkingLotDetailDTO dto)
        {
            var parkingDetail = await dbContext.ParkingLotDetails.FirstOrDefaultAsync(pd => pd.Id == dto.ParkingLotDetailId)
                ?? throw new NotFoundException($"Not found Parking Detail {dto.ParkingLotDetailId}");

            bool isUpdated = false;
            if (dto.Status.HasValue)
            {
                parkingDetail.Status = dto.Status.Value
                    ? "active"   
                    : "inactive"; 
                isUpdated = true;
            }
            if (dto.Checking.HasValue)
            {
                parkingDetail.Checking = dto.Checking.Value;
                isUpdated = true;
            }
            if (!isUpdated)
                throw new BadRequestException("No fields to update. Provide at least one of Status or Checking.");

            await dbContext.SaveChangesAsync();
            return $"Updated ParkingLotDetail {dto.ParkingLotDetailId}.";
        }

        public async Task<Guid> DeleteParkingLotDetailAsync(Guid id)
        {
            var parkingLotDetail = await dbContext.ParkingLotDetails.FirstOrDefaultAsync(pd => pd.Id == id) 
                ?? throw new NotFoundException($"Not found parking lot detail {id}");
            var parkingLot = await dbContext.ParkingLots
                .FirstOrDefaultAsync(pl => pl.Id == parkingLotDetail.ParkingLotId)
                ?? throw new NotFoundException($"Not found parking lot {parkingLotDetail.ParkingLotId}");
            dbContext.ParkingLotDetails.Remove(parkingLotDetail);
            dbContext.ParkingLots.Remove(parkingLot);
            await dbContext.SaveChangesAsync();
            return parkingLotDetail.Id;
        }

        public async Task UpdateOverdueParkingBillsAsync()
        {
            var cutoff = DateTime.UtcNow.AddDays(-15);
            var billsToOverdue = await dbContext.Payments
                .Where(b => b.Status == "pending" && b.CreateDate <= cutoff)
                .ToListAsync();

            if (!billsToOverdue.Any())
                return;

            // Set overdue status
            billsToOverdue.ForEach(b => b.Status = "overdue");

            // Lock related parking cards
            var parkingLotIds = billsToOverdue
                .Where(b => b.ParkingId != null)
                .Select(b => b.ParkingId.Value)
                .Distinct()
                .ToList();

            var parkingDetails = await dbContext.ParkingLotDetails
                .Where(p => parkingLotIds.Contains(p.ParkingLotId.Value))
                .ToListAsync();

            parkingDetails.ForEach(p => p.Status = "inactive");

            // Prepare notifications
            foreach (var bill in billsToOverdue)
            {
                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    Title = $"Quá hạn thanh toán tiền bãi đỗ xe tháng {bill.CreateDate.Month}",
                    Content = $"Quý cư dân thân mến, hóa đơn tiền bãi đỗ xe tháng {bill.CreateDate.Month} ({bill.Amount:N0} VND) đã quá hạn thanh toán. " +
                              "Vui lòng thực hiện thanh toán trong thời gian sớm nhất để đảm bảo quyền lợi và tránh các gián đoạn không mong muốn.",
                    Type = "Notice",
                    CreatedBy = bill.UserId,
                    CreatedAt = DateTime.UtcNow
                };

                dbContext.Notifications.Add(notification);

                dbContext.NotificationReceivers.Add(new NotificationReceiver
                {
                    Id = Guid.NewGuid(),
                    NotificationId = notification.Id,
                    UserId = bill.UserId,
                    Receiver = "resident",
                    IsRead = false,
                    ReadAt = null
                });

                var deviceTokens = await dbContext.UserDevices
                    .Where(ud => ud.UserId == bill.UserId && ud.IsActive)
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

            // Commit all changes
            await dbContext.SaveChangesAsync();
        }

        public async Task CreateRecurringParkingBillAsync(Guid previousInvoiceId)
        {
            var old = await dbContext.Payments
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == previousInvoiceId && p.Status == "completed");

            if (old == null) return; 

            // Lấy lại các thông tin cần
            var parkingLotId = old.ParkingId ?? throw new Exception("Missing ParkingLotId");
            var userId = old.UserId;

            var price = await dbContext.PriceParkingLots.AsNoTracking().FirstOrDefaultAsync();
            if (price == null) throw new Exception("Price config not found");

            var details = await dbContext.ParkingLotDetails
                .Where(d => d.ParkingLotId == parkingLotId)
                .Select(d => new { d.Type })
                .ToListAsync();

            decimal totalAmount = details.Sum(d => d.Type == "car"
                ? (price.PricePerOto ?? throw new Exception("PricePerOto missing"))
                : (price.PricePerMotor ?? throw new Exception("PricePerMotor missing")));

            // Tạo hóa đơn mới
            var newPayment = new Payment
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ParkingId = parkingLotId,
                Amount = totalAmount,
                Status = "pending",
                Method = "VietQR",
                CreateDate = DateTime.UtcNow
            };

            await dbContext.Payments.AddAsync(newPayment);
            await dbContext.SaveChangesAsync();
        }

        public async Task<List<ParkingViewDTO>> GetAllParkingAsync(bool? status, int? day, int? month)
        {
            var query = dbContext.Payments
                .Include(p => p.User)
                    .ThenInclude(u => u.Apartments)
                .Include(p => p.Parking)
                .AsQueryable();

            if (day.HasValue)
            {
                var fromDate = DateTime.UtcNow.AddDays(-day.Value);
                query = query.Where(p => p.CreateDate <= fromDate);
            }

            if (month.HasValue && month.Value >= 1 && month.Value <= 12)
            {
                query = query.Where(p => p.CreateDate.Month == month.Value);
            }

            if (status.HasValue)
            {
                if (status.Value)
                    query = query.Where(p => p.Status == "completed");
                else
                    query = query.Where(p => p.Status != "completed");
            }

            query = query.Where(p => p.ParkingId.HasValue);
            var result = await query
                .OrderByDescending(p => p.CreateDate)
                .Select(p => new ParkingViewDTO
                {
                    ParkingId = p.ParkingId.Value,
                    FullName = p.User.FullName,
                    ApartmentNumber = p.User.Apartments.FirstOrDefault().ApartmentNumber,
                    Amount = p.Amount,
                    CreateDate = p.CreateDate,
                    Email = p.User.Email,
                    PhoneNumber = p.User.PhoneNumber,
                    Status = p.Status
                })
                .ToListAsync();
            return result;
        }

        public async Task<List<ParkingLotViewDTO>> GetMyParkingAsync(Guid userId)
        {
            var flat = await dbContext.ParkingLotDetails
                .AsNoTracking()
                .Where(d => d.ParkingLot.UserId == userId)
                .Select(d => new
                {
                    ParkingId = d.ParkingLot.Id,
                    FullName = d.ParkingLot.User.FullName,
                    Apartment = d.ParkingLot.User.Apartments
                                        .OrderBy(a => a.Id)
                                        .Select(a => a.ApartmentNumber)
                                        .FirstOrDefault(),
                    Type = d.Type,
                    Accept = d.ParkingLot.Accept
                })
                .ToListAsync();

            var grouped = flat
                .GroupBy(x => x.ParkingId)
                .Select(g => new ParkingLotViewDTO
                {
                    ParkingId = g.Key,
                    Name = g.First().FullName,
                    Apartment = g.First().Apartment,
                    NumberOfMotorbike = g.Count(x => x.Type == "motor"),
                    NumberOfCar = g.Count(x => x.Type == "car"),
                    Accept = g.First().Accept
                })
                .ToList();

            return grouped;
        }

        public async Task GenerateMonthlyBillsAsync() //Đúng ngày đầu mỗi tháng sẽ có hóa đơn
        {
            // Lấy tất cả các parking lot hiện có
            var parkingLots = await dbContext.ParkingLots
                .Select(p => new { p.Id, p.UserId })
                .ToListAsync();

            var price = await dbContext.PriceParkingLots.AsNoTracking().FirstOrDefaultAsync()
                       ?? throw new Exception("Price config not found");

            foreach (var lot in parkingLots)
            {
                // Tính tổng số thẻ và tiền
                var details = await dbContext.ParkingLotDetails
                    .Where(d => d.ParkingLotId == lot.Id && d.Status == "active")
                    .Select(d => d.Type)
                    .ToListAsync();

                decimal total = details.Sum(type =>
                    type == "car" ? (price.PricePerOto ?? 0)
                                  : (price.PricePerMotor ?? 0)
                );

                var invoice = new Payment
                {
                    Id = Guid.NewGuid(),
                    UserId = lot.UserId.Value,
                    ParkingId = lot.Id,
                    Amount = total,
                    Status = "pending",
                    Method = "VietQR",
                    CreateDate = DateTime.UtcNow
                };

                await dbContext.Payments.AddAsync(invoice);
            }

            await dbContext.SaveChangesAsync();
        }

        public async Task<List<ParkingLotViewDTO>> GetAllParkingLotRequestAsync()
        {
            var flat = await dbContext.ParkingLotDetails
                .AsNoTracking()
                .Where(d => d.Status == "pending")
                .Select(d => new
                {
                    ParkingId = d.ParkingLot.Id,
                    FullName = d.ParkingLot.User.FullName,
                    Apartment = d.ParkingLot.User.Apartments
                                        .OrderBy(a => a.Id)
                                        .Select(a => a.ApartmentNumber)
                                        .FirstOrDefault(),
                    Type = d.Type,
                    Accept = d.ParkingLot.Accept
                })
                .ToListAsync();

            var grouped = flat
                .GroupBy(x => x.ParkingId)
                .Select(g => new ParkingLotViewDTO
                {
                    ParkingId = g.Key,
                    Name = g.First().FullName,
                    Apartment = g.First().Apartment,
                    NumberOfMotorbike = g.Count(x => x.Type == "motor"),
                    NumberOfCar = g.Count(x => x.Type == "car"),
                    Accept = g.Select(x => x.Accept).FirstOrDefault()
                })
                .ToList();

            return grouped;
        }
    }
}

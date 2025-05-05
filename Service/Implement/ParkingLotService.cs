using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
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
    public class ParkingLotService : IParkingLotService
    {
        private readonly ApartmentDbContext dbContext;

        public ParkingLotService(ApartmentDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task<List<Guid>> AddParkingLotAsync(ParkingCardRequestDTO dtos, Guid userId)
        {
            if (dtos == null)
                throw new BadRequestException("Request body can't empty !");
            int motors = dtos.NumberOfMotorbikes ?? 0;
            int cars = dtos.NumberOfCars ?? 0;
            int total = motors + cars;
            if (total == 0)
                throw new BadRequestException("You need to provide at least one card to create ! ");

            //ParkingLot 
            var parkingLotId = Guid.NewGuid();
            var parkingLot = new ParkingLot
            {
                Id = parkingLotId,
                UserId = userId,
                Accept = false
            };

            //ParkingLotDetail
            var details = new List<ParkingLotDetail>(total);
            for (int i = 0; i < motors; i++)
            {
                details.Add(new ParkingLotDetail
                {
                    Id = Guid.NewGuid(),
                    Type = "motor",
                    Status = "pending",
                    Checking = false,
                    ParkingLotId = parkingLotId
                });
            }
            for (int i = 0; i < cars; i++)
            {
                details.Add(new ParkingLotDetail
                {
                    Id = Guid.NewGuid(),
                    Type = "car",
                    Status = "pending",
                    Checking = false,
                    ParkingLotId = parkingLotId
                });
            }

            var tracker = dbContext.ChangeTracker;
            bool prevAutoDetect = tracker.AutoDetectChangesEnabled;
            try
            {
                tracker.AutoDetectChangesEnabled = false;
                await dbContext.ParkingLots.AddAsync(parkingLot);
                await dbContext.ParkingLotDetails.AddRangeAsync(details);
                await dbContext.SaveChangesAsync();
            }
            finally
            {
                tracker.AutoDetectChangesEnabled = prevAutoDetect;
            }
            return details.Select(d => d.Id).ToList();
        }

        public async Task<List<ParkingLotViewDTO>> GetAllParkingLotsAsync()
        {
            var flat = await dbContext.ParkingLotDetails
                .AsNoTracking()
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

                //Bulk-update status
                var updatedCount = await detailQuery.ExecuteUpdateAsync(s => s
                    .SetProperty(d => d.Status, _ => "active")
                );

                //Get Price
                var price = await dbContext.PriceParkingLots
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
                if (price == null)
                    throw new InvalidOperationException("Price settings not found.");

                // total price
                var details = await detailQuery
                    .Select(d => new { d.Type })
                    .ToListAsync();

                decimal totalAmount = details
                    .Sum(d => d.Type == "car"
                        ? (price.PricePerOto ?? throw new ConflictException("PricePerOto is null."))
                        : (price.PricePerMotor ?? throw new ConflictException("PricePerMotor is null.")));

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

                // Commit
                await tx.CommitAsync();

                return $"Accepted {updatedCount} card(s). Invoice {invoice.Id} created for {totalAmount:C}.";
            }
            else
            {
                var deleted = await detailQuery.ExecuteDeleteAsync();
                return $"Reject success: deleted {deleted} card(s).";
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
    }
}

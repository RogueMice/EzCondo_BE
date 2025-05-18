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
    public class OtherServiceService : IOtherService
    {
        private readonly ApartmentDbContext dbContext;

        public OtherServiceService(ApartmentDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task<Guid?> AddOrUpdateAsync(OtherServiceDTO dto)
        {
            var otherSerivce = await dbContext.OtherServices.FirstOrDefaultAsync(x => x.Id == dto.Id) ?? new EzCondo_Data.Domain.OtherService();

            //add
            if (otherSerivce.Id == Guid.Empty)
            {
                otherSerivce.Id = Guid.NewGuid();
                otherSerivce.Name = dto.Name;
                otherSerivce.Price = dto.Price;
                otherSerivce.Description = dto.Description;
                await dbContext.AddAsync(otherSerivce);
            }
            //update
            else
            {
                otherSerivce.Name = dto.Name;
                otherSerivce.Price = dto.Price;
                otherSerivce.Description = dto.Description;
                dbContext.OtherServices.Update(otherSerivce);
            }
            await dbContext.SaveChangesAsync();
            return otherSerivce.Id;
        }

        public async Task<Guid?> DeleteOtherServiceAsync(Guid otherSerivceId)
        {
            var otherService = await dbContext.OtherServices.FirstOrDefaultAsync(x => x.Id == otherSerivceId) ?? throw new NotFoundException($"OtherService with id {otherSerivceId} not found");
            dbContext.OtherServices.Remove(otherService);
            await dbContext.SaveChangesAsync();
            return otherService.Id;
        }

        public Task<List<OtherServiceDTO>> GetOtherServiceAsync()
        {
            var otherServices = dbContext.OtherServices.AsNoTracking().Select(x => new OtherServiceDTO
            {
                Id = x.Id,
                Name = x.Name,
                Price = x.Price.Value,
                Description = x.Description
            }).ToListAsync();
            return otherServices;
        }

        public async Task GenerateMonthlyBillsAsync() //Đúng ngày đầu mỗi tháng sẽ có hóa đơn
        {
            var otherService = await dbContext.OtherServices.ToListAsync();

            decimal total = 0;
            foreach (var item in otherService)
            {
                total += item.Price.Value;
            }

            var resident = await dbContext.Users
                .Where(u => u.Role.Name.ToLower() == "resident")
                .ToListAsync();
            foreach (var item in resident)
            {
                var invoice = new Payment
                {
                    Id = Guid.NewGuid(),
                    UserId = item.Id,
                    Amount = total,
                    Status = "pending",
                    Method = "VietQR",
                    CreateDate = DateTime.UtcNow
                };
                await dbContext.Payments.AddAsync(invoice);
            }

            await dbContext.SaveChangesAsync();
        }
    }
}

using EzCondo_Data.Context;
using EzCondo_Data.Domain;
using EzConDo_Service.CloudinaryIntegration;
using EzConDo_Service.DTO;
using EzConDo_Service.Interface;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace EzConDo_Service.Implement
{
    public class ServiceOfSerivce : IService_service
    {
        private readonly ApartmentDbContext dbContext;

        public ServiceOfSerivce(ApartmentDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task<Guid> AddServiceAsync(AddServiceDTO serviceDTO)
        {
            var service = new EzCondo_Data.Domain.Service
            {
                Id = Guid.NewGuid(),
                ServiceName = serviceDTO.ServiceName,
                Description = serviceDTO.Description,
                BillingType = serviceDTO.BillingType.ToLower(),
                Price = serviceDTO.Price,
                Status = "active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };
            dbContext.Services.Add(service);
            await dbContext.SaveChangesAsync();

            return service.Id;
        }
    }
}

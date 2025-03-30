using EzCondo_Data.Context;
using EzConDo_Service.CloudinaryIntegration;
using EzConDo_Service.DTO;
using EzConDo_Service.Interface;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using static EzConDo_Service.ExceptionsConfig.CustomException;

namespace EzConDo_Service.Implement
{
    public class ServiceOfSerivce : IService_service
    {
        private readonly ApartmentDbContext dbContext;

        public ServiceOfSerivce(ApartmentDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Guid> AddOrUpdateServiceAsync(AddServiceDTO serviceDTO)
        {
            var service = await dbContext.Services.FirstOrDefaultAsync(x => x.Id == serviceDTO.Id) ?? new EzCondo_Data.Context.Service();
            //add new
            if (service.Id == Guid.Empty)
            {
                service.Id = Guid.NewGuid();
                service.ServiceName = serviceDTO.ServiceName;
                service.Description = serviceDTO.Description;
                service.TypeOfMonth = serviceDTO.TypeOfMonth;
                service.TypeOfYear = serviceDTO.TypeOfYear;
                service.PriceOfMonth = serviceDTO.PriceOfMonth;
                service.PriceOfYear = serviceDTO.PriceOfYear;
                service.Status = serviceDTO.Status ?? "active";
                service.CreatedAt = DateTime.UtcNow;
                service.UpdatedAt = null;
                dbContext.Services.Add(service);
                await dbContext.SaveChangesAsync();
                return service.Id;
            }

            //update
            service.ServiceName = serviceDTO.ServiceName;
            service.Description = serviceDTO.Description;
            service.TypeOfMonth = serviceDTO.TypeOfMonth;
            service.TypeOfYear = serviceDTO.TypeOfYear;
            service.PriceOfMonth = serviceDTO.PriceOfMonth;
            service.PriceOfYear = serviceDTO.PriceOfYear;
            service.Status = serviceDTO.Status.ToLower() ?? service.Status;
            service.UpdatedAt = DateTime.UtcNow;

            dbContext.Services.Update(service);
            await dbContext.SaveChangesAsync();
            return serviceDTO.Id;
        }


        public async Task<ServiceViewDTO> GetServiceByIdAsync(Guid serviceId)
        {
            var service = await dbContext.Services.AsNoTracking().FirstOrDefaultAsync(x => x.Id == serviceId) ?? throw new NotFoundException("ServiceId is not correct !!");
            var serviceView = new ServiceViewDTO
            {
                Id = service.Id,
                ServiceName = service.ServiceName,
                Description = service.Description,
                TypeOfMonth = service.TypeOfMonth,
                TypeOfYear = service.TypeOfYear,
                PriceOfMonth = service.PriceOfMonth,
                PriceOfYear = service.PriceOfYear,
                Status = service.Status,
                CreatedAt = service.CreatedAt,
                UpdatedAt = service.UpdatedAt
            };
            return serviceView;
        }

        public async Task<List<ServiceViewDTO>> GetAllServicesAsync(string? serviceName, bool? status)
        {
            var query = dbContext.Services.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(serviceName))
            {
                query = query.Where(x => x.ServiceName.Contains(serviceName));
            }

            if (status.HasValue)
            {
                query = query.Where(x => x.Status == (status.Value ? "active" : "inactive"));
            }

            var services = await query.Select(x => new ServiceViewDTO
            {
                Id = x.Id,
                ServiceName = x.ServiceName,
                Description = x.Description,
                TypeOfMonth = x.TypeOfMonth,
                TypeOfYear = x.TypeOfYear,
                PriceOfMonth = x.PriceOfMonth,
                PriceOfYear = x.PriceOfYear,
                Status = x.Status,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            }).ToListAsync();

            return services;
        }
    }
}

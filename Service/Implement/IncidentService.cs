using EzCondo_Data.Context;
using EzCondo_Data.Domain;
using EzConDo_Service.DTO;
using EzConDo_Service.Interface;
using FirebaseAdmin.Auth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EzConDo_Service.ExceptionsConfig.CustomException;

namespace EzConDo_Service.Implement
{
    public class IncidentService : I_incidentService
    {
        private readonly ApartmentDbContext dbContext;

        public IncidentService(ApartmentDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task<Guid?> AddAsync(IncidentDTO dto, Guid userId)
        {
            int priority;
            switch (dto.Type?.ToLower())
            {
                case "security":
                    priority = 1;
                    break;
                case "technical":
                    priority = 2;
                    break;
                case "infrastructure":
                    priority = 2;
                    break;
                case "complaint":
                    priority = 3;
                    break;
                case "other":
                    priority = 3;
                    break;
                default:
                    throw new ArgumentException("The type of incidents is invalid.");
            }

            var incident = new Incident
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Description = dto.Description,
                Priority = priority,
                ReportedAt = DateTime.UtcNow,
                Status = "pending",
                Title = dto.Title,
                Type = dto.Type
            };

            dbContext.Add(incident);
            await dbContext.SaveChangesAsync();

            //Send notification to managers
            var notification = new CreateNotificationDTO
            {
                Id = Guid.NewGuid(),
                Title = $"New incident reported {dto.Title}",
                Content = $"A new incident has been reported by {userId}.",
                Type = "incident",
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                Receiver = "manager"
            };

            await CreateNotificationAsync(notification,userId);

            return incident.Id;
        }

        public async Task CreateNotificationAsync(CreateNotificationDTO notificationDTO, Guid userId)
        {
            var notificationReceiver = notificationDTO.Receiver.ToLower();

            var roleExists = await dbContext.Roles
                .AnyAsync(r => r.Name.ToLower() == notificationReceiver)
                .ConfigureAwait(false);

            if (!roleExists)
            {
                throw new NotFoundException($"Role '{notificationDTO.Type}' is not found !");
            }

            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                Title = notificationDTO.Title,
                Content = notificationDTO.Content,
                Type = notificationDTO.Type,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            //Get userId of Managers
            var managerUserIds = await dbContext.Users
                .Where(u => u.Role.Name.ToLower() == notificationReceiver)
                .Select(u => u.Id)
                .ToListAsync()
                .ConfigureAwait(false);

            var receivers = managerUserIds.Select(managerId => new NotificationReceiver
            {
                Id = Guid.NewGuid(),
                NotificationId = notification.Id,
                UserId = managerId,
                Receiver = notificationReceiver,
                IsRead = false,
                ReadAt = null
            }).ToList();

            await using var transaction = await dbContext.Database.BeginTransactionAsync().ConfigureAwait(false);

            try
            {
                dbContext.Notifications.Add(notification);
                dbContext.NotificationReceivers.AddRange(receivers);

                await dbContext.SaveChangesAsync().ConfigureAwait(false);
                await transaction.CommitAsync().ConfigureAwait(false);
            }
            catch
            {
                await transaction.RollbackAsync().ConfigureAwait(false);
                throw;
            }
        }

        public async Task<string> EditAsync(UpdateIncidentDTO dto)
        {
            var incident = await dbContext.Incidents.FirstOrDefaultAsync(i => i.Id == dto.IncidentId) ?? throw new NotFoundException($"incident id {dto.IncidentId} not found!");

            var status = dto.Status.ToLower();
            if (status != "pending" && status != "underway" && status != "resolved")
            {
                throw new ConflictException("The status of incident is invalid.");
            }

            incident.Status = dto.Status;

            dbContext.Update(incident);
            await dbContext.SaveChangesAsync();
            return "Update incident successfully!";
        }

        public async Task<IncidentDTO> GetIncidentByUserIdAsync(Guid userId)
        {
            var incident = await dbContext.Incidents.AsNoTracking()
                .Where(i => i.UserId == userId)
                .Select(i => new IncidentDTO
                {
                    Id = i.Id,
                    Title = i.Title,
                    Description = i.Description,
                    Type = i.Type,
                    Priority = i.Priority,
                    ReportedAt = i.ReportedAt,
                    Status = i.Status,
                    UserId = i.UserId
                }).FirstOrDefaultAsync();

            return incident;
        }

        public Task<List<IncidentDTO>> GetIncidentsAsync()
        {
            var incidents = dbContext.Incidents.AsNoTracking()
                .OrderBy(i => i.Priority) //sort priority increase
                .Select(i => new IncidentDTO
            {
                Id = i.Id,
                Title = i.Title,
                Description = i.Description,
                Type = i.Type,
                Priority = i.Priority,
                ReportedAt = i.ReportedAt,
                Status = i.Status,
                UserId = i.UserId
            }).ToListAsync();
            return incidents;
        }
    }
}

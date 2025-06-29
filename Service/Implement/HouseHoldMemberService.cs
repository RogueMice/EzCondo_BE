﻿using EzCondo_Data.Context;
using EzCondo_Data.Domain;
using EzConDo_Service.DTO;
using EzConDo_Service.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static EzConDo_Service.ExceptionsConfig.CustomException;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EzConDo_Service.Implement
{
    public class HouseHoldMemberService : IHouseHoldMemberService
    {
        private readonly ApartmentDbContext dbContext;

        public HouseHoldMemberService(ApartmentDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Guid?> AddOrUpdateAsync(HouseHoldMemberDTO houseHoldMemberDTO)
        {
            var apartment = dbContext.Apartments.FirstOrDefault(x => x.ApartmentNumber == houseHoldMemberDTO.ApartmentNumber) ?? throw new NotFoundException($"Apartment number: {houseHoldMemberDTO.ApartmentNumber} not found !");

            var houseHoldMember = dbContext.HouseHoldMembers.FirstOrDefault(x => x.Id == houseHoldMemberDTO.Id) ?? new HouseHoldMember();

            //find Check phone and no
            var normalizedPhone = houseHoldMemberDTO.PhoneNumber.Trim().ToLowerInvariant();
            var normalizedNo = houseHoldMemberDTO.No.Trim().ToLowerInvariant();

            bool phoneExists = await dbContext.HouseHoldMembers.AnyAsync(x => x.PhoneNumber == normalizedPhone && x.Id != houseHoldMember.Id);
            if (phoneExists)
            {
                throw new BadRequestException("Phone number already exist !");
            }

            bool noNumberExists = await dbContext.HouseHoldMembers.AnyAsync(x => x.No == normalizedNo && x.Id != houseHoldMember.Id);
            if (noNumberExists)
            {
                throw new BadRequestException("No number already exist !");
            }

            //add
            if (houseHoldMember.Id == Guid.Empty)
            {
                houseHoldMember.Id = Guid.NewGuid();
                houseHoldMember.No = houseHoldMemberDTO.No;
                houseHoldMember.FullName = houseHoldMemberDTO.FullName;
                houseHoldMember.DateOfBirth = houseHoldMemberDTO.DateOfBirth;
                houseHoldMember.Gender = houseHoldMemberDTO.Gender;
                houseHoldMember.PhoneNumber = houseHoldMemberDTO.PhoneNumber;
                houseHoldMember.Relationship = houseHoldMemberDTO.Relationship;
                houseHoldMember.CreateAt = DateTime.UtcNow;
                
                houseHoldMember.UserId = apartment.UserId ?? throw new InvalidOperationException("Apartment UserId cannot be null.");
                dbContext.HouseHoldMembers.Add(houseHoldMember);

                //Upgrade Resident_number
                apartment.ResidentNumber += 1;
            }
            //update
            else
            {
                houseHoldMember.No = houseHoldMemberDTO.No;
                houseHoldMember.FullName = houseHoldMemberDTO.FullName;
                houseHoldMember.DateOfBirth = houseHoldMemberDTO.DateOfBirth;
                houseHoldMember.Gender = houseHoldMemberDTO.Gender;
                houseHoldMember.PhoneNumber = houseHoldMemberDTO.PhoneNumber;
                houseHoldMember.Relationship = houseHoldMemberDTO.Relationship;
            }
            await dbContext.SaveChangesAsync();
            return houseHoldMember.Id;
        }

        public async Task<Guid?> DeleteAsync(Guid id)
        {
            var houseHoldMember = await dbContext.HouseHoldMembers.Where(x => x.Id == id).ExecuteDeleteAsync();
            if (houseHoldMember == 0)
                throw new NotFoundException($"House hold member id: {id} not found!");
            return id;
        }

        public async Task<MyHouseHoldResponseDTO> GetMyHoldHouseMemberAsync(Guid user_id)
        {
            var user = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == user_id) ?? throw new NotFoundException($"User id: {user_id} not found !");

            var houseHoldMembers = await dbContext.HouseHoldMembers
                                   .AsNoTracking()
                                   .Where(x => x.UserId == user_id)
                                   .Select(x => new MyHouseHoldMemberDTO
                                   {
                                       Id = x.Id,
                                       No = x.No,
                                       FullName = x.FullName,
                                       DateOfBirth = x.DateOfBirth,
                                       Gender = x.Gender,
                                       PhoneNumber = x.PhoneNumber,
                                       Relationship = x.Relationship
                                   }).ToListAsync();
            return new MyHouseHoldResponseDTO
            {
                User = new UserDTO
                {
                    Id = user.Id,
                    Regency = "Householder",
                    FullName = user.FullName,
                    DateOfBirth = user.DateOfBirth,
                    Gender = user.Gender,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber
                },
                Members = houseHoldMembers
            };
        }

        public async Task<List<MyHouseHoldMemberDTO>> GetHoldHouseMemberByApartmentNumberAsync(string apartmentNumber)
        {
            var normalizedApartmentNumber = apartmentNumber.ToLower();
            var apartment = await dbContext.Apartments
                                           .AsNoTracking()
                                           .FirstOrDefaultAsync(x => x.ApartmentNumber.ToLower() == normalizedApartmentNumber)
                                           ?? throw new NotFoundException($"Apartment number: {apartmentNumber} not found!");

            var userId = apartment.UserId ?? throw new NotFoundException($"User id: {apartment.UserId} not found!");
            var userTask = await dbContext.Users
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == userId)
                        ?? throw new NotFoundException($"User id: {userId} not found!");

            var houseHoldMembersTask = await dbContext.HouseHoldMembers
                                   .AsNoTracking()
                                   .Where(x => x.UserId == userId)
                                   .Select(x => new MyHouseHoldMemberDTO
                                   {
                                       Id = x.Id,
                                       No = x.No,
                                       FullName = x.FullName,
                                       DateOfBirth = x.DateOfBirth,
                                       Gender = x.Gender,
                                       PhoneNumber = x.PhoneNumber,
                                       Relationship = x.Relationship ?? string.Empty
                                   }).ToListAsync();

            return houseHoldMembersTask;
        }

        public async Task<GenerateDashboardDTO> GetHoldHouseMemberAsync()
        {
            // Determine Vietnam time zone
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

            // Get current date in Vietnam time
            var todayInVietnam = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone).Date;

            // Calculate the start of this week (Monday) and the exclusive end (next Monday) in Vietnam time
            int daysSinceMonday = ((int)todayInVietnam.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            var startOfWeekInVietnam = todayInVietnam.AddDays(-daysSinceMonday);
            var startOfNextWeekInVietnam = startOfWeekInVietnam.AddDays(7);

            // Convert week boundaries back to UTC for database queries
            var startOfWeekUtc = TimeZoneInfo.ConvertTimeToUtc(startOfWeekInVietnam, vietnamTimeZone);
            var endOfLastWeekUtc = startOfWeekUtc;          // exclusive boundary for last week
            var endOfThisWeekUtc = TimeZoneInfo.ConvertTimeToUtc(startOfNextWeekInVietnam, vietnamTimeZone); // exclusive boundary for this week

            // Filter for regular users (exclude admins and managers)
            Expression<Func<User, bool>> isRegularUser = u =>
                u.Role.Name.ToLower() != "admin" && u.Role.Name.ToLower() != "manager";

            // Count total residents (owners + household members) at the end of last week
            var lastWeekOwners = await dbContext.Users
                .Where(isRegularUser)
                .CountAsync(u => u.CreateAt < endOfLastWeekUtc);
            var lastWeekMembers = await dbContext.HouseHoldMembers
                .CountAsync(m => m.CreateAt < endOfLastWeekUtc);
            var totalLastWeek = lastWeekOwners + lastWeekMembers;

            // Count total residents at the end of this week
            var thisWeekOwners = await dbContext.Users
                .Where(isRegularUser)
                .CountAsync(u => u.CreateAt < endOfThisWeekUtc);
            var thisWeekMembers = await dbContext.HouseHoldMembers
                .CountAsync(m => m.CreateAt < endOfThisWeekUtc);
            var totalThisWeek = thisWeekOwners + thisWeekMembers;

            // Calculate growth rate
            double growthRate;
            if (totalLastWeek > 0)
                growthRate = (double)(totalThisWeek - totalLastWeek) / totalLastWeek * 100;
            else
                growthRate = totalThisWeek > 0 ? 100.0 : 0.0;
            growthRate = Math.Round(growthRate, 1);

            var trendDescription = growthRate >= 0
                ? $"Increased compared to last week"
                : $"Decreased compared to last week";

            // Count current total residents (owners + household members)
            var currentOwners = await dbContext.Users
                .Where(isRegularUser)
                .CountAsync();
            var currentMembers = await dbContext.HouseHoldMembers
                .CountAsync();
            var totalResidents = currentOwners + currentMembers;

            return new GenerateDashboardDTO
            {
                Total = totalResidents,
                Increase = totalThisWeek - totalLastWeek,
                GrowthRatePercent = growthRate,
                TrendDescription = trendDescription,
                ApartmentThisWeek = Math.Round((double)totalThisWeek / totalResidents * 100, 1),
                ApartmentLastWeek = Math.Round((double) totalLastWeek / totalResidents * 100, 1)
            };
        }
    }
}

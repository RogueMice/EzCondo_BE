using EzCondo_Data.Context;
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
                houseHoldMember.UserId = apartment.UserId;
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

        public Task<HouseHoldMemberDTO> GetAsync(Guid user_id)
        {
            throw new NotImplementedException();
        }
    }
}

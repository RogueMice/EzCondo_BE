﻿using CloudinaryDotNet.Actions;
using EzCondo_Data.Context;
using EzCondo_Data.Domain;
using EzConDo_Service.CloudinaryIntegration;
using EzConDo_Service.DTO;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using Newtonsoft.Json.Linq;
using Service.IService;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using System.Runtime.Intrinsics.Arm;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static EzConDo_Service.ExceptionsConfig.CustomException;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Service.Service
{
    public class UserService : IUserService
    {
        private readonly EzCondo_Data.Context.ApartmentDbContext dbContext;
        private readonly IPasswordHasher<User> passwordHasher;
        private readonly IConfiguration configuration;
        private readonly CloudinaryService cloudinaryService;
        private readonly IMemoryCache memoryCache;
        private static readonly Random _random = new Random();

        public UserService(EzCondo_Data.Context.ApartmentDbContext dbContext, IPasswordHasher<User> passwordHasher,
            IConfiguration configuration, CloudinaryService cloudinaryService, IMemoryCache memoryCache)
        {
            this.dbContext = dbContext;
            this.passwordHasher = passwordHasher;
            this.configuration = configuration;
            this.cloudinaryService = cloudinaryService;
            this.memoryCache = memoryCache;
        }

        public async Task<UserViewDTO> ValidateUserAsync(LoginDTO dto)
        {
            var user = await dbContext.Users
                .SingleOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower())
                ?? throw new NotFoundException("User not found.");

            if (user.Status.Equals("inactive", StringComparison.OrdinalIgnoreCase))
                throw new LockedException("Account blocked.");

            if (passwordHasher.VerifyHashedPassword(user, user.Password, dto.Password)
                != PasswordVerificationResult.Success)
                return null;

            await dbContext.Entry(user).Reference(u => u.Role).LoadAsync();
            await dbContext.Entry(user).Collection(u => u.Apartments).LoadAsync();

            user.TokenVersion = Guid.NewGuid();
            await dbContext.SaveChangesAsync(); 

            return new UserViewDTO
            {
                Id = user.Id,
                FullName = user.FullName,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                ApartmentNumber = user.Apartments.FirstOrDefault()?.ApartmentNumber ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                Status = user.Status,
                RoleName = user.Role.Name,
                TokenVersion = user.TokenVersion.Value // Non-null after assignment
            };
        }

        public static string HashPassword(string password)
        {
            var passwordHasher = new PasswordHasher<object>();
            return passwordHasher.HashPassword(null, password);
        }

        public async Task<Guid> AddUserAsync(AddUserDTO userDTO)
        {
            var normalizedRoleName = userDTO.RoleName.Trim().ToUpperInvariant();
            var normalizedEmail = userDTO.Email.Trim().ToLowerInvariant();
            var normalizedPhone = userDTO.PhoneNumber.Trim().ToLowerInvariant();
            var normalizedApartmentNumber = userDTO.ApartmentNumber.Trim().ToLowerInvariant();
            //find role and check email
            var roleEmailPhoneCheck = await dbContext.Roles
                                    .Where(r => r.Name.ToUpper() == normalizedRoleName)
                                    .Select(r => new
                                    {
                                        RoleId = r.Id,
                                        EmailExists = dbContext.Users.Any(u => u.Email.ToLower() == normalizedEmail),
                                        PhoneExists = dbContext.Users.Any(u => u.PhoneNumber == userDTO.PhoneNumber),
                                    })
                                    .FirstOrDefaultAsync();

            if (roleEmailPhoneCheck == null)
                throw new NotFoundException($"Role '{userDTO.RoleName}' not found !");

            if (roleEmailPhoneCheck.EmailExists)
                throw new ConflictException($"Email '{userDTO.Email}' existed");

            if (roleEmailPhoneCheck.PhoneExists)
                throw new ConflictException($"Phone '{userDTO.PhoneNumber}' existed");

            //Check apartment table (userId == null)
            var apartment = await dbContext.Apartments.FirstOrDefaultAsync(a => a.ApartmentNumber == normalizedApartmentNumber) ?? throw new NotFoundException($"Apartment number '{userDTO.ApartmentNumber}' not found !");
            if(apartment.UserId != null)
                throw new ConflictException($"Apartment number '{userDTO.ApartmentNumber}' is already in use!");

            string randomPassword = GenerateRandomPassword();
            var passwordHash = passwordHasher.HashPassword(null, randomPassword);
            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = userDTO.FullName,
                DateOfBirth = userDTO.DateOfBirth,
                Email = userDTO.Email,
                Gender = userDTO.Gender,
                PhoneNumber = userDTO.PhoneNumber,
                Password = passwordHash,
                Status = "Active",
                CreateAt = DateTime.UtcNow,
                RoleId = roleEmailPhoneCheck.RoleId
            };

            dbContext.Users.Add(user);

            //update apartment  
            apartment.UserId = user.Id;
            apartment.ResidentNumber = 1;
            dbContext.Apartments.Update(apartment);
            await dbContext.SaveChangesAsync();

            Task.Run(() => SendWelcomeEmailAsync(user.Email, user.FullName, userDTO.RoleName, randomPassword));
            return user.Id;
        }

        private string GenerateRandomPassword(int length = 8)
        {
            const string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@$?_-";
            Random random = new Random();
            return new string(Enumerable.Repeat(validChars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private async Task SendWelcomeEmailAsync(string email, string fullName, string roleName, string password)
        {
            string subject = "Thông tin tài khoản của bạn";
            string body = $@"
            Xin chào {fullName},<br><br>
            Tài khoản <b>{roleName}</b> của bạn đã được tạo thành công!<br>
            🔹 <b>Username</b>: {email}<br>
            🔹 <b>Mật khẩu</b>: {password}<br>
            Vui lòng đổi mật khẩu sau khi đăng nhập.<br>
            Trân trọng!";

            await SendEmailAsync(email, subject, body);
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var emailSettings = configuration.GetSection("EmailSettings");
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("Admin", emailSettings["SenderEmail"]));
            email.To.Add(new MailboxAddress("", toEmail));
            email.Subject = subject;

            email.Body = new TextPart("html")
            {
                Text = body
            };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(emailSettings["SmtpServer"], int.Parse(emailSettings["Port"]), MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(emailSettings["SenderEmail"], emailSettings["SenderPassword"]);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        public async Task<Guid> UpdateUserAsync(UpdateUserDTO userDTO)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userDTO.Id) ?? throw new NotFoundException($"UserId {userDTO.Id} not found !");

            bool phoneExists = await dbContext.Users.AnyAsync(u => u.PhoneNumber == userDTO.PhoneNumber && u.Id != userDTO.Id);
            if (phoneExists)
            {
                throw new ConflictException($"Phone number '{userDTO.PhoneNumber}' is already in use!");
            }

            user.FullName = userDTO.FullName;
            user.DateOfBirth = userDTO.DateOfBirth;
            user.Gender = userDTO.Gender;
            user.PhoneNumber = userDTO.PhoneNumber;
            user.Status = userDTO.Status;
            user.UpdateAt = DateTime.UtcNow;

            bool apartmentNumberExists = await dbContext.Apartments.AnyAsync(
                                                                        a => a.ApartmentNumber == userDTO.ApartmentNumber);
            if(!apartmentNumberExists)
            {
                throw new NotFoundException($"Apartment Number {userDTO.ApartmentNumber} not found !");
            }

            bool apartmentNumberExistsAndEmpty = await dbContext.Apartments.AnyAsync(
                                                                        a => a.ApartmentNumber == userDTO.ApartmentNumber
                                                                        && a.UserId != userDTO.Id);
            if (apartmentNumberExistsAndEmpty)
            {
                throw new ConflictException($"Apartment number '{userDTO.ApartmentNumber}' is already in use!");
            }
            if(userDTO.Status.ToLower() == "active" || userDTO.Status.ToLower() == "inactive")
            {
                var apartment = await dbContext.Apartments.FirstOrDefaultAsync(a => a.UserId == userDTO.Id);
                apartment.ApartmentNumber = userDTO.ApartmentNumber;

                await dbContext.SaveChangesAsync();
                return userDTO.Id;
            }
            throw new NotFoundException($"Status: {userDTO.Status} is not found");
        }

        public async Task<Guid> DeleteUserAsync(Guid userId)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId)
                        ?? throw new NotFoundException($"UserId {userId} not found !");

            var apartment = await dbContext.Apartments.FirstOrDefaultAsync(u => u.UserId == userId);
            var citizen = await dbContext.Citizens.FirstOrDefaultAsync(c => c.UserId == userId);
            var notificationReceivers = await dbContext.NotificationReceivers.Where(n => n.UserId == userId).ToListAsync();

            if (apartment != null)
            {
                apartment.UserId = null;
                dbContext.Apartments.Update(apartment);
            }

            if (citizen != null)
            {
                dbContext.Citizens.Remove(citizen);
            }

            if (notificationReceivers != null && notificationReceivers.Any())
            {
                dbContext.NotificationReceivers.RemoveRange(notificationReceivers);
            }

            dbContext.Users.Remove(user);
            await dbContext.SaveChangesAsync();
            return user.Id;
        }

        public async Task ForgotPasswordAsync(string email)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email) ?? throw new NotFoundException($"Email {email} not found !"); ;

            // Create code 
            string code = _random.Next(100000, 999999).ToString();
            var expireAt = DateTime.UtcNow.AddMinutes(5);

            var resetCode = await dbContext.PasswordResetCodes.FirstOrDefaultAsync(r => r.UserId == user.Id);
            if (resetCode == null)
            {
                resetCode = new PasswordResetCode
                {
                    UserId = user.Id,
                    Code = code,
                    ExpireAt = expireAt
                };
                await dbContext.PasswordResetCodes.AddAsync(resetCode);
            }
            else
            {
                resetCode.Code = code;
                resetCode.ExpireAt = expireAt;
            }

            await dbContext.SaveChangesAsync();

            // Send email have code
            var subject = "Yêu cầu đặt lại mật khẩu";
            var body = $@"Mã xác nhận của bạn là: {code}.</br>
                  Mã này có hiệu lực trong 5 phút.";

            // Offload gửi email sang background task để API trả về nhanh chóng
            _ = Task.Run(() => SendEmailAsync(user.Email, subject, body));

            return;
        }

        public async Task<string>VerifyOTPAsync(string email, string Code)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email) ?? throw new NotFoundException($"Email {email} not found !"); ;

            var resetCode = await dbContext.PasswordResetCodes
                                       .FirstOrDefaultAsync(r => r.UserId == user.Id
                                       && r.Code == Code
                                       && r.ExpireAt > DateTime.UtcNow)
                                       ?? throw new ConflictException("Confirmation code is not valid or expired!");

            //Tạo tokenMemory để lưu vào Cache
            var tokenMemory = Guid.NewGuid().ToString() + ".RogueMiceMemoryTokenSetting1215125qwrqwrqwrqwrasdasda";
            //Lưu thông tin vào Cache
            memoryCache.Set(tokenMemory, email, TimeSpan.FromMinutes(5));

            return tokenMemory;
        }

        public async Task<string> ResetPasswordAsync(string tokenMemory, string newPassword)
        {
            //Get Email from token in cache
            if(!memoryCache.TryGetValue(tokenMemory, out string email))
            {
                throw new ConflictException("Token is invalid or expired!");
            }

            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email) ?? throw new NotFoundException($"Email {email} not found !"); ;

            user.Password = HashPassword(newPassword);

            var resetCode = await dbContext.PasswordResetCodes
                            .FirstOrDefaultAsync(r => r.UserId == user.Id);
            dbContext.PasswordResetCodes.Remove(resetCode);

            await dbContext.SaveChangesAsync();

            //Delete tokenMemory out cache
            memoryCache.Remove(tokenMemory);
            return "Reset password is successful!!";
        }

        public async Task<List<UserViewDTO>> GetUsersAsync(string? roleName, string? search)
        {
            var query = from u in dbContext.Users.AsNoTracking()
                        join a in dbContext.Apartments.AsNoTracking() on u.Id equals a.UserId
                        into ua
                        from a in ua.DefaultIfEmpty()
                        where (string.IsNullOrEmpty(roleName) || u.Role.Name == roleName)
                              && (string.IsNullOrEmpty(search) ||
                                  u.FullName.Contains(search) ||
                                  (a != null && a.ApartmentNumber.Contains(search)) ||
                                  u.PhoneNumber.Contains(search))
                        select new UserViewDTO
                        {
                            Id = u.Id,
                            FullName = u.FullName,
                            DateOfBirth = u.DateOfBirth,
                            Gender = u.Gender,
                            ApartmentNumber = a != null ? a.ApartmentNumber : string.Empty,
                            PhoneNumber = u.PhoneNumber,
                            Email = u.Email,
                            Status = u.Status,
                            RoleName = u.Role.Name
                        };

            return await query.ToListAsync();
        }

        public async Task<CurrentUserDTO?> GetCurrentUserInfoAsync(Guid userId)
        {
            var query = from u in dbContext.Users.AsNoTracking()
                        join a in dbContext.Apartments.AsNoTracking() on u.Id equals a.UserId
                        into ua
                        from a in ua.DefaultIfEmpty()
                        join c in dbContext.Citizens.AsNoTracking() on u.Id equals c.UserId
                        into uc
                        from c in uc.DefaultIfEmpty()
                        where userId == u.Id
                        select new CurrentUserDTO
                        {
                            Id = u.Id,
                            FullName = u.FullName,
                            DateOfBirth = u.DateOfBirth,
                            Gender = u.Gender,
                            ApartmentNumber = a != null ? a.ApartmentNumber : string.Empty,
                            PhoneNumber = u.PhoneNumber,
                            Email = u.Email,
                            Status = u.Status,
                            RoleName = u.Role.Name,
                            Avatar = u.Avatar,
                            No = c != null ? c.No : string.Empty
                        };
            return await query.FirstOrDefaultAsync();
        }

        public async Task<EditUserDTO?> EditCurrentUserInforAsync(EditUserDTO userDTO)
        {
            var user = await dbContext.Users.FindAsync(userDTO.Id) ?? throw new NotFoundException($"UserId {userDTO.Id} not found !"); ;
            bool phoneExists = await dbContext.Users.AnyAsync(u => u.PhoneNumber == userDTO.PhoneNumber && u.Id != userDTO.Id);
            if (phoneExists)
            {
                throw new ConflictException($"Phone number '{userDTO.PhoneNumber}' is already in use!");
            }

            user.FullName = userDTO.FullName;
            user.PhoneNumber = userDTO.PhoneNumber;
            user.Gender = userDTO.Gender;
            user.UpdateAt = DateTime.UtcNow;
            user.DateOfBirth = DateOnly.FromDateTime(userDTO.DateOfBirth);

            await dbContext.SaveChangesAsync();
            return userDTO;
        }

        public async Task<bool> AddOrUpdateAvtAsync(Guid userId, IFormFile avt)
        {
            var user = await dbContext.Users.FindAsync(userId) ?? throw new NotFoundException($"UserId {userId} not found !"); ;
            if (user == null)
                return false;
            if (avt != null && avt.Length > 0)
            {
                if (!string.IsNullOrEmpty(user.Avatar))
                {
                    await cloudinaryService.DeleteImageAsync(user.Avatar);
                }
                user.Avatar = await cloudinaryService.UploadImageAsync(avt);
            }
            await dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<GetUserByIdDTO?> GetUserByIdDTOAsync(Guid userId)
        {
            var query = from u in dbContext.Users.AsNoTracking()
                        join a in dbContext.Apartments.AsNoTracking() on u.Id equals a.UserId
                        into ua
                        from a in ua.DefaultIfEmpty()
                        join c in dbContext.Citizens.AsNoTracking() on u.Id equals c.UserId
                        into uc
                        from c in uc.DefaultIfEmpty()
                        where userId == u.Id
                        select new GetUserByIdDTO
                        {
                            Id = u.Id,
                            FullName = u.FullName,
                            DateOfBirth = u.DateOfBirth,
                            Gender = u.Gender,
                            ApartmentNumber = a != null ? a.ApartmentNumber : string.Empty,
                            PhoneNumber = u.PhoneNumber,
                            Email = u.Email,
                            Status = u.Status,
                            RoleName = u.Role.Name,
                            No = c != null ? c.No : string.Empty,
                            DateOfIssue = c != null ? c.DateOfIssue : default,
                            DateOfExpiry = c != null ? (DateOnly)c.DateOfExpiry : default,
                            FrontImage = c != null ? c.FrontImage : string.Empty,
                            BackImage = c != null ? c.BackImage : string.Empty
                        };
            return await query.FirstOrDefaultAsync();
        }

        public async Task<string?> ChangePasswordAsync(ChangePasswordDTO dto)
        {
            var user = await dbContext.Users.FindAsync(dto.UserId) ?? throw new NotFoundException($"UserId {dto.UserId} not found !");
            var passwordCurrent = passwordHasher.VerifyHashedPassword(user, user.Password, dto.OldPassword);
            if (passwordCurrent != PasswordVerificationResult.Success)
                throw new ConflictException("Old password is incorrect !");

            //success and change_password
            user.Password = HashPassword(dto.NewPassword);
            dbContext.Users.Update(user);
            await dbContext.SaveChangesAsync();
            return "Change password successfully!";
        }
    }
}

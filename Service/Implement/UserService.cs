using CloudinaryDotNet.Actions;
using EzCondo_Data.Context;
using EzCondo_Data.Domain;
using EzConDo_Service.DTO;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using Service.IService;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Service.Service
{
    public class UserService : IUserService
    {
        private readonly EzCondo_Data.Context.ApartmentDbContext dbContext;
        private readonly IPasswordHasher<User> passwordHasher;
        private readonly IConfiguration configuration;
        private static readonly Random _random = new Random();

        public UserService(EzCondo_Data.Context.ApartmentDbContext dbContext, IPasswordHasher<User> passwordHasher,
            IConfiguration configuration)
        {
            this.dbContext = dbContext;
            this.passwordHasher = passwordHasher;
            this.configuration = configuration;
        }

        public async Task<UserViewDTO> ValidateUserAsync(LoginDTO dto)
        {
            var user = await dbContext.Users
                 .AsNoTracking()
                 .Include(u => u.Role)
                 .Include(u => u.Apartments)
                 .SingleOrDefaultAsync(u => u.Email == dto.Email) 
                 ?? throw new Exception("User not found");

            var result = passwordHasher.VerifyHashedPassword(user, user.Password, dto.Password);
            if (result != PasswordVerificationResult.Success)
                return null;

            return new UserViewDTO
            {
                Id = user.Id,
                FullName = user.FullName,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                ApartmentNumber = user.Apartments?.FirstOrDefault()?.ApartmentNumber ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                email = user.Email,
                Status = user.Status,
                RoleName = user.Role.Name
            };
        }

        public static string HashPassword(string password)
        {
            var passwordHasher = new PasswordHasher<object>();
            return passwordHasher.HashPassword(null, password);
        }

        public async Task<List<UserViewDTO>> GetUsersAsync(string? roleName)
        {
            var query = from u in dbContext.Users.AsNoTracking()
                        join a in dbContext.Apartments.AsNoTracking() on u.Id equals a.UserId into ua
                        from a in ua.DefaultIfEmpty()
                        where string.IsNullOrEmpty(roleName) || u.Role.Name == roleName
                        select new UserViewDTO
                        {
                            Id = u.Id,
                            FullName = u.FullName,
                            DateOfBirth = u.DateOfBirth,
                            Gender = u.Gender,
                            ApartmentNumber = a != null ? a.ApartmentNumber : string.Empty,
                            PhoneNumber = u.PhoneNumber,
                            email = u.Email,
                            Status = u.Status,
                            RoleName = u.Role.Name
                        };

            return await query.ToListAsync();
        }

        public async Task<Guid> AddUserAsync(AddUserDTO userDTO)
        {
            var normalizedRoleName = userDTO.RoleName.Trim().ToUpperInvariant();
            var normalizedEmail = userDTO.Email.Trim().ToLowerInvariant();
            //find role and check email
            var roleAndEmailCheck = await dbContext.Roles
                                    .Where(r => r.Name.ToUpper() == normalizedRoleName)
                                    .Select(r => new { RoleId = r.Id, EmailExists = dbContext.Users.Any(u => u.Email.ToLower() == normalizedEmail) })
                                    .FirstOrDefaultAsync();

            if (roleAndEmailCheck == null)
                throw new InvalidOperationException($"Role '{userDTO.RoleName}' invalid");

            if (roleAndEmailCheck.EmailExists)
                throw new InvalidOperationException($"Email '{userDTO.Email}' existed");

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
                RoleId = roleAndEmailCheck.RoleId
            };
            var apartment = new Apartment
            {
                Id = Guid.NewGuid(),
                ApartmentNumber = userDTO.ApartmentNumber,
                ResidentNumber = 1,
                Acreage = 100,
                Description = "Nothing ....",
                UserId = user.Id
            };

            dbContext.Users.Add(user);
            dbContext.Apartments.Add(apartment);
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
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userDTO.Id) ?? throw new Exception("User Id invalid !");

            user.FullName = userDTO.FullName;
            user.DateOfBirth = userDTO.DateOfBirth;
            user.Gender = userDTO.Gender;
            user.PhoneNumber = userDTO.PhoneNumber;
            user.Status = userDTO.Status;
            user.UpdateAt = DateTime.UtcNow;

            var apartment = await dbContext.Apartments.FirstOrDefaultAsync(u => u.UserId == userDTO.Id);

            if (apartment is not null)
            {
                apartment.ApartmentNumber = userDTO.ApartmentNumber;
            }
            else
            {
                apartment = new Apartment()
                {
                    Id = Guid.NewGuid(),
                    ApartmentNumber = userDTO.ApartmentNumber,
                    ResidentNumber = 1,
                    Acreage = 100,
                    Description = "Nothing ....",
                    UserId = user.Id
                };
                dbContext.Apartments.Add(apartment);
            }

            await dbContext.SaveChangesAsync();
            return userDTO.Id;
        }

        public async Task<Guid> DeleteUserAsync(Guid userId)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId) ?? throw new Exception("User Id invalid !");

            var apartment = await dbContext.Apartments.FirstOrDefaultAsync(u => u.UserId == userId);
            if (apartment is not null)
            {
                dbContext.Apartments.Remove(apartment);
            }

            var citizen = await dbContext.Citizens.FirstOrDefaultAsync(c => c.UserId == userId);
            if (citizen is not null)
            {
                dbContext.Citizens.Remove(citizen);
            }

            dbContext.Users.Remove(user);
            await dbContext.SaveChangesAsync();
            return user.Id;
        }

        public async Task ForgotPasswordAsync(string email)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email) ?? throw new Exception("User Id invalid !");

            // Create code 
            string code = _random.Next(100000, 999999).ToString();
            var expireAt = DateTime.UtcNow.AddMinutes(5);

            var resetCode = await dbContext.PasswordResetCodes.FirstOrDefaultAsync(r => r.UserId == user.Id);
            if (resetCode == null)
            {
                // Chưa có -> thêm mới
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
                // Đã có -> ghi đè code cũ
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

        public async Task<string> GetPasswordAsync(ResetPasswordWithCodeDTO dto)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == dto.Email) ?? throw new Exception("User invalid !");

            var resetCode = await dbContext.PasswordResetCodes
                                        .FirstOrDefaultAsync(r => r.UserId == user.Id
                                        && r.Code == dto.Code
                                        && r.ExpireAt > DateTime.UtcNow)
                                        ?? throw new Exception("Confirmation code is not valid or expired!");

            //Thay đổi pass và mã hóa mật khẩu
            user.Password = HashPassword(dto.NewPassword);
            //Xóa code đã sử dụng trong db
            dbContext.PasswordResetCodes.Remove(resetCode);

            await dbContext.SaveChangesAsync();
            return "Reset password is successful!!";
        }
    }
}

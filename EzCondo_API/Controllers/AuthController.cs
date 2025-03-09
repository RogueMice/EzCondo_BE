using EzCondo_Data.Domain;
using EzConDo_Service.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Service.IService;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EzCondo_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly IUserService userService;

        public AuthController(IConfiguration configuration, IUserService userService)
        {
            this.configuration = configuration;
            this.userService = userService;
        }

        [HttpGet("Test Authorize"), Authorize]
        public async Task<IActionResult> Get()
        {
            return Ok("Success");
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            var user = await userService.ValidateUserAsync(dto);
            if (user is null)
                return Unauthorized("Email or password is not true ... Please login again!");
            var token = CreateToken(user);
            var response = new
            {
                data = new
                {
                    user = user,
                    token = token
                }
            };
            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("Forgot-Password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDTO dto)
        {
            await userService.ForgotPasswordAsync(dto.Email);
            return Ok("Sent Otp to your email");
        }

        [AllowAnonymous]
        [HttpPost("Reset-Password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordWithCodeDTO dto)
        {
            return Ok(await userService.GetPasswordAsync(dto));
        }

        private string CreateToken(UserViewDTO user)
        {
            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name,user.FullName),
                new Claim(ClaimTypes.Role, user.RoleName)
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                configuration.GetSection("Appsettings:Token").Value));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: cred);
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return (jwt);
        }
    }
}

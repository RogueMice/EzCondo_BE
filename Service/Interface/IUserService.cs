using EzConDo_Service.DTO;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.IService
{
    public interface IUserService
    {
        Task<UserViewDTO> ValidateUserAsync(LoginDTO dto);
        Task<List<UserViewDTO>> GetUsersAsync(string? roleName, string? search);

        Task<Guid> AddUserAsync(AddUserDTO userDTO);

        Task<Guid> UpdateUserAsync(UpdateUserDTO userDTO);

        Task<Guid> DeleteUserAsync(Guid userId);

        Task ForgotPasswordAsync(string email);

        Task<string> GetPasswordAsync(ResetPasswordWithCodeDTO dto);

        Task<CurrentUserDTO?> GetCurrentUserInfoAsync(Guid userId);

        Task<EditUserDTO?> EditCurrentUserInforAsync(EditUserDTO userDTO);

        Task<bool> AddOrUpdateAvtAsync(Guid userId,IFormFile avt);

        Task <GetUserByIdDTO?> GetUserByIdDTOAsync(Guid userId);

        Task <string?>ChangePasswordAsync(ChangePasswordDTO dto);
    }
}

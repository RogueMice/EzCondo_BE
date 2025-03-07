using EzCondo_Data.Domain;
using EzConDo_Service.DTO;
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
        Task<List<UserViewDTO>> GetUsersAsync(string? roleName);

        Task<Guid> AddUserAsync(AddUserDTO userDTO);

        Task<Guid> UpdateUserAsync(UpdateUserDTO userDTO);

        Task<Guid> DeleteUserAsync(Guid userId);

        Task ForgotPasswordAsync(string email);

        Task<string> GetPasswordAsync(ResetPasswordWithCodeDTO dto);

    }
}

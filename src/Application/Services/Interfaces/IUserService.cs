using TiendaUCN.src.Application.DTOs.AuthDTO;
using TiendaUCN.src.Domain.Models;

namespace TiendaUCN.src.Application.Services.Interfaces
{
    public interface IUserService
    {
        Task<string> RegisterAsync(RegisterDTO registerDTO);
        Task EmailVerificationAsync(EmailVerificationDTO emailVerificationDTO);
        Task<string> LoginAsync(LoginDTO loginDTO);
        Task<string> LogoutAsync(string token);
        Task<int> DeleteUnconfirmedUsersAsync();
        Task<string> ResendVerificationCodeAsync(ResendVerificationCodeDTO resendVerificationCodeDTO);
    }
}

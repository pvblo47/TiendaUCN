using TiendaUCN.src.Domain.Models;

namespace TiendaUCN.src.Application.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(User user, string roleName);
        Task AddToBlacklistAsync(string token);
        Task<bool> IsTokenBlacklistedAsync(string token);
        Task<int> DeleteExpiredTokensInBlacklistAsync();
    }
}
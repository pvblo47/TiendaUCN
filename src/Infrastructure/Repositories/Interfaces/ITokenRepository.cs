using TiendaUCN.src.Domain.Models;

namespace TiendaUCN.src.Infrastructure.Repositories.Interfaces
{
    public interface ITokenRepository
    {
        Task AddAsync(BlacklistedToken token);
        Task<bool> IsBlacklistedAsync(string tokenId);
        Task<int> DeleteExpiredTokensAsync();
    }
}
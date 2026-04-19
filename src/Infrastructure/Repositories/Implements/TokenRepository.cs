using Microsoft.EntityFrameworkCore;
using TiendaUCN.src.Domain.Models;
using TiendaUCN.src.Infrastructure.Data;
using TiendaUCN.src.Infrastructure.Repositories.Interfaces;

namespace TiendaUCN.src.Infrastructure.Repositories.Implements
{
    public class TokenRepository : ITokenRepository
    {
        private readonly DataContext _context;
        public TokenRepository(DataContext context)
        {
            _context = context;
        }

        public async Task AddAsync(BlacklistedToken token)
        {
            await _context.BlacklistedTokens.AddAsync(token);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsBlacklistedAsync(string tokenId)
        {
            return await _context.BlacklistedTokens.AnyAsync(u => u.TokenId == tokenId);
        }

        public async Task<int> DeleteExpiredTokensAsync()
        {
            // Elimina los tokens que han expirado, sin soft delete
            var now = DateTime.UtcNow;
            return await _context.BlacklistedTokens
                .Where(t => t.ExpireAt < now)
                .ExecuteDeleteAsync();
        }
    }
}
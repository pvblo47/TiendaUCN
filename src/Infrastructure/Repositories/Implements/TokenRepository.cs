using TiendaUCN.src.Infrastructure.Repositories.Interfaces;

namespace TiendaUCN.src.Infrastructure.Repositories.Implements
{
    /// <summary>
    /// Implementación provisional del repositorio de tokens.
    /// TODO: Implementar persistencia real de tokens (base de datos, cache, etc.).
    /// </summary>
    public class TokenRepository : ITokenRepository
    {
        public Task SaveTokenAsync(int userId, string token)
        {
            // TODO: Implementar persistencia real
            return Task.CompletedTask;
        }

        public Task<bool> IsTokenValidAsync(string token)
        {
            // TODO: Implementar validación real
            return Task.FromResult(!string.IsNullOrEmpty(token));
        }

        public Task RevokeTokenAsync(string token)
        {
            // TODO: Implementar revocación real
            return Task.CompletedTask;
        }
    }
}

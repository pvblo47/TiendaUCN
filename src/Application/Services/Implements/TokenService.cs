using TiendaUCN.src.Application.Services.Interfaces;

namespace TiendaUCN.src.Application.Services.Implements
{
    /// <summary>
    /// Implementación provisional del servicio de tokens.
    /// TODO: Implementar lógica real de generación y validación de tokens (JWT, etc.).
    /// </summary>
    public class TokenService : ITokenService
    {
        public Task<string> GenerateTokenAsync(int userId)
        {
            // TODO: Implementar generación de token real (JWT)
            var token = Guid.NewGuid().ToString();
            return Task.FromResult(token);
        }

        public Task<bool> ValidateTokenAsync(string token)
        {
            // TODO: Implementar validación de token real
            return Task.FromResult(!string.IsNullOrEmpty(token));
        }
    }
}

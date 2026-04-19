using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TiendaUCN.src.Application.Services.Interfaces;
using TiendaUCN.src.Domain.Models;
using TiendaUCN.src.Infrastructure.Repositories.Interfaces;

namespace TiendaUCN.src.Application.Services.Implements
{
    public class TokenService : ITokenService
    {
        private readonly string _jwtSecret;
        private readonly ITokenRepository _tokenRepository;
        private readonly IConfiguration _configuration;
        private readonly int _tokenExpirationInHours;

        public TokenService(ITokenRepository tokenRepository, IConfiguration configuration)
        {
            _tokenRepository = tokenRepository;
            _configuration = configuration;
            _tokenExpirationInHours = int.Parse(_configuration["Token:ExpirationTimeInHours"] ?? throw new InvalidOperationException("Token expiration time is not configured."));
            _jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? throw new InvalidOperationException("JWT secret key is not configured.");
        }
        public string GenerateToken(User user, string roleName)
        {
            try
            {
                // Listamos los claims que queremos incluir en el token (solo las necesarias, no todas las propiedades del usuario)
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email!),
                    new Claim(ClaimTypes.Role, roleName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Reclamación del ID único del token
                };

                // Creamos la clave de seguridad
                var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_jwtSecret));

                // Creamos las credenciales de firma, ojo la clave debe ser lo suficientemente larga y segura (256 bits mínimo para HMACSHA256) que son 32 caracteres
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                // Creamos el token
                var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(_tokenExpirationInHours), // El token expirará en el tiempo configurado
                    signingCredentials: creds
                );

                // Serializamos el token a string
                Log.Information("Token JWT generado exitosamente para el usuario {UserId}", user.Id);
                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al generar el token JWT para el usuario {UserId}", user.Id);
                throw new InvalidOperationException("Error al generar el token JWT", ex);
            }
        }

        public async Task AddToBlacklistAsync(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            // Extrae el jti qeu representa el ID único del token y la fecha de expiración
            var jti = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value
                ?? throw new InvalidOperationException("El token no contiene un jti válido para agregar a la blacklist");
            var expireAt = jwtToken.ValidTo;

            // Verifica si el token ya está en la blacklist antes de agregarlo
            var isBlacklisted = await _tokenRepository.IsBlacklistedAsync(jti);
            if (isBlacklisted)
            {
                Log.Warning("Intento de agregar a blacklist un token que ya está en la blacklist: {TokenId}", jti);
                throw new InvalidOperationException("El token ya está en la blacklist.");
            }

            // Mappea el token a un modelo de blacklist
            var blacklistedToken = new BlacklistedToken
            {
                TokenId = jti,
                ExpireAt = expireAt
            };

            // Almacena en la blacklist
            await _tokenRepository.AddAsync(blacklistedToken);
        }

        public async Task<bool> IsTokenBlacklistedAsync(string token)
        {
            // Lee el token JWT para extraer el jti
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            // Extrae el jti del token
            var jti = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;

            // Verifica si el jti está en la blacklist
            if (jti != null)
            {
                var isBlacklisted = await _tokenRepository.IsBlacklistedAsync(jti);
                return isBlacklisted;
            }

            Log.Warning("El token no contiene un jti válido para verificar en la blacklist");
            throw new InvalidOperationException("El token no contiene un jti válido.");
        }

        public async Task<int> DeleteExpiredTokensInBlacklistAsync()
        {
            int deletedCount = await _tokenRepository.DeleteExpiredTokensAsync();
            Log.Information("Tokens expirados eliminados de la blacklist: {DeletedCount}", deletedCount);
            return deletedCount;
        }
    }
}
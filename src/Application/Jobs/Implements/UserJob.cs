using Hangfire;
using Serilog;
using TiendaUCN.src.Application.Jobs.Interfaces;
using TiendaUCN.src.Application.Services.Interfaces;

namespace TiendaUCN.src.Application.Jobs.Implements
{
    public class UserJob : IUserJob
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;
        public UserJob(IUserService userService, ITokenService tokenService)
        {
            _userService = userService;
            _tokenService = tokenService;
        }

        [AutomaticRetry(Attempts = 10, DelaysInSeconds = new int[] { 60, 120, 300, 600, 900 })]
        public async Task DeleteExpiredTokensInBlacklistAsync()
        {
            Log.Information("Eliminando tokens expirados en la blacklist...");
            await _tokenService.DeleteExpiredTokensInBlacklistAsync();
        }

        // Configura el trabajo para que se reintente automáticamente en caso de fallo, con un número máximo de intentos y retrasos entre ellos
        [AutomaticRetry(Attempts = 10, DelaysInSeconds = new int[] { 60, 120, 300, 600, 900 })]
        public async Task DeleteUnconfirmedUsersAsync()
        {
            Log.Information("Eliminando usuarios no confirmados...");
            await _userService.DeleteUnconfirmedUsersAsync();
        }
    }
}
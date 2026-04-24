using Mapster;
using Serilog;
using TiendaUCN.src.Application.DTOs.AuthDTO;
using TiendaUCN.src.Application.Services.Interfaces;
using TiendaUCN.src.Domain.Models;
using TiendaUCN.src.Infrastructure.Repositories.Implements;
using TiendaUCN.src.Infrastructure.Repositories.Interfaces;

namespace TiendaUCN.src.Application.Services.Implements
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IVerificationCodeRepository _verificationCodeRepository;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        private readonly int _verificationCodeExpiry;
        private readonly int _maxFailedEmailVerificationAttempts;
        private readonly int _waitingTimeInMinutesAfterResendEmail;
        private readonly int _daysToDeleteUnverifiedAccount;

        public UserService(IEmailService emailService, IUserRepository userRepository, IVerificationCodeRepository verificationCodeRepository, IConfiguration configuration, ITokenService tokenService)
        {
            _emailService = emailService;
            _userRepository = userRepository;
            _verificationCodeRepository = verificationCodeRepository;
            _configuration = configuration;
            _tokenService = tokenService;
            _verificationCodeExpiry = _configuration.GetValue<int>("VerificationCode:ExpirationTimeInMinutes");
            _maxFailedEmailVerificationAttempts = _configuration.GetValue<int>("VerificationCode:MaxFailedAttempts");
            _waitingTimeInMinutesAfterResendEmail = _configuration.GetValue<int>("VerificationCode:WaitingTimeInMinutesAfterResendEmail");
            _daysToDeleteUnverifiedAccount = _configuration.GetValue<int>("Jobs:DaysToDeleteUnverifiedAccount");
        }

        public async Task<string> RegisterAsync(RegisterDTO registerDTO)
        {
            // Validar si el usuario ya existe por nombre
            bool isRegisteredByName = await _userRepository.ExistsByNameAsync(registerDTO.Name);
            if (isRegisteredByName)
            {
                Log.Warning($"El usuario {registerDTO.Name} ya existe");
                throw new InvalidOperationException("El usuario ya existe");
            }

            // Validar si el usuario ya existe por email
            bool isRegisteredByEmail = await _userRepository.ExistsByEmailAsync(registerDTO.Email);
            if (isRegisteredByEmail)
            {
                Log.Warning($"El usuario con el correo {registerDTO.Email} ya existe");
                throw new InvalidOperationException("El correo ya esta registrado");
            }

            // Validar si el usuario ya existe por RUT
            bool isRegisteredByRut = await _userRepository.ExistsByRutAsync(registerDTO.Rut);
            if (isRegisteredByRut)
            {
                Log.Warning($"El usuario con el RUT {registerDTO.Rut} ya existe");
                throw new InvalidOperationException("El RUT ya esta registrado");
            }

            // Validar si el usuario ya existe por numero de teléfono
            bool isRegisteredByPhoneNumber = await _userRepository.ExistsByPhoneNumberAsync(registerDTO.PhoneNumber);
            if (isRegisteredByPhoneNumber)
            {
                Log.Warning($"El usuario con el numero de teléfono {registerDTO.PhoneNumber} ya está registrado.");
                throw new InvalidCastException("El numero de teléfono ya está registrado.");
            }

            // Crear el usuario
            var user = registerDTO.Adapt<User>();
            await _userRepository.CreateAsync(user);

            Log.Information($"Registro exitoso para el usuario: {user.Email} con Id: {user.Id}");

            // Generar y enviar el código de verificación
            var (verificationCode, verificationCodeExpiry) = await GenerateCodeAndExpiryAsync();

            // Crear la entidad de VerificationCode
            var verificationCodeEntity = new VerificationCode
            {
                Code = verificationCode,
                Expiry = verificationCodeExpiry,
                UserId = user.Id
            };

            // Guardar el código de verificación en la base de datos
            var createdVerificationCode = await _verificationCodeRepository.CreateAsync(verificationCodeEntity);
            Log.Information($"Código de verificación generado para el usuario: {user.Email} - Código: {createdVerificationCode.Code}");

            await _emailService.SendVerificationCodeEmailAsync(user.Email, createdVerificationCode.Code);
            Log.Information($"Se ha enviado un código de verificación al correo electrónico: {user.Email}");

            return $"Se ha enviado un código de verificación a su correo electrónico, este código expirara en {_verificationCodeExpiry} minutos";

        }

        public async Task EmailVerificationAsync(EmailVerificationDTO emailVerificationDTO)
        {
            // Obtener el usuario por correo electrónico
            User? user = await _userRepository.GetByEmailAsync(emailVerificationDTO.Email);
            if (user == null)
            {
                Log.Warning($"Intento de verificación fallido: No se encontró un usuario con el correo {emailVerificationDTO.Email}");
                throw new KeyNotFoundException("No se encontró un usuario con el correo proporcionado.");
            }

            // Validar si el correo electrónico ya está verificado
            if (user.EmailConfirmed)
            {
                Log.Warning($"Intento de verificación fallido: El correo electrónico ya está verificado para el usuario {user.Email}");
                throw new InvalidOperationException("El correo electrónico ya está verificado.");
            }

            // Verificar el número de intentos fallidos de verificación de correo para el usuario
            if (user.VerificationCode.FailedAttempts >= _maxFailedEmailVerificationAttempts)
            {
                Log.Warning($"Intento de verificación fallido: Demasiados intentos fallidos para el usuario {user.Email}");
                throw new InvalidOperationException("Demasiados intentos fallidos. Tu cuenta sera eliminada.");
            }

            // Validar el código de verificación, solo si el número de intentos fallidos es menor al máximo permitido
            if (user.VerificationCode.Code != emailVerificationDTO.VerificationCode)
            {
                // Incrementar el contador de intentos fallidos de verificación de correo para el usuario
                var isUpdated = await _verificationCodeRepository.UpdateFailedAttemptsAsync(user.VerificationCode.Id);
                if (!isUpdated)
                {
                    Log.Error($"Error al actualizar el contador de intentos fallidos de verificación de correo para el usuario {user.Email}");
                    throw new InvalidOperationException("Error al actualizar el contador de intentos fallidos.");
                }

                // Calcular los intentos disponibles después de actualizar el contador de intentos fallidos
                var failedAttemptsAfterUpdate = user.VerificationCode.FailedAttempts + 1;
                var availableAttempts = _maxFailedEmailVerificationAttempts - failedAttemptsAfterUpdate;

                Log.Warning($"Intento de verificación fallido: Código de verificación incorrecto para el usuario {user.Email}");
                throw new InvalidOperationException("Código de verificación incorrecto, tienes " + availableAttempts + " intentos disponibles antes de que tu cuenta sea eliminada.");
            }

            // Validar la expiración del código de verificación, solo si el código es correcto
            // Evitar que se genere un nuevo código si el usuario aún no ha ingresado el código correcto
            if (user.VerificationCode.Expiry < DateTime.UtcNow)
            {
                Log.Warning($"Intento de verificación fallido: Código de verificación expirado para el usuario {user.Email}");

                // Generar y enviar un nuevo código de verificación
                var (verificationCode, verificationCodeExpiry) = await GenerateCodeAndExpiryAsync();
                var isUpdated = await _verificationCodeRepository.UpdateAsync(user.VerificationCode.Id, verificationCode, verificationCodeExpiry);
                if (!isUpdated)
                {
                    Log.Error($"Error al actualizar el código de verificación para el usuario {user.Email}");
                    throw new InvalidOperationException("Error al actualizar el código de verificación.");
                }

                await _emailService.SendVerificationCodeEmailAsync(user.Email, verificationCode);
                Log.Information($"Se ha enviado un código de verificación al correo electrónico: {user.Email}");

                throw new InvalidOperationException("El código de verificación ha expirado, se ha enviado un nuevo código a su correo electrónico.");
            }

            // Marcar el correo electrónico como verificado
            bool isVerified = await _userRepository.MarkEmailAsVerifiedAsync(user.Id);
            if (!isVerified)
            {
                Log.Error($"Error al marcar el correo electrónico como verificado para el usuario {user.Email}");
                throw new InvalidOperationException("Error al verificar el correo electrónico.");
            }

            // Enviar correo de bienvenida
            await _emailService.SendWelcomeEmailAsync(user.Email);

            Log.Information($"Correo electrónico verificado exitosamente para el usuario {user.Email}");
        }
        private async Task<(string, DateTime)> GenerateCodeAndExpiryAsync()
        {
            // Generar un codigo de verificación y su fecha de expiración
            string verificationCode = new Random().Next(100000, 999999).ToString();
            DateTime verificationCodeExpiry = DateTime.UtcNow.AddMinutes(_verificationCodeExpiry);

            return await Task.FromResult((verificationCode, verificationCodeExpiry));
        }

        public async Task<string> LogoutAsync(string token)
        {
            // Validar que se haya proporcionado un token
            if (string.IsNullOrEmpty(token))

                Log.Warning("Intento de logout fallido: Token no proporcionado");
            throw new ArgumentException("Token es requerido para el logout.");

            // Agregar el token a la blacklist
            await _tokenService.AddToBlacklistAsync(token);

            Log.Information($"Token JWT agregado a la blacklist: {token}");
            return "Logout exitoso.";
        }

        public async Task<string> LoginAsync(LoginDTO loginDTO)
        {
            // obtener el usuario por correo electrónico
            User user = await _userRepository.GetByEmailAsync(loginDTO.Email)
            ?? throw new KeyNotFoundException("Credenciales invalidas");

            // Validar la contraseña
            if (!BCrypt.Net.BCrypt.Verify(loginDTO.Password, user.PasswordHash))
            {

                Log.Warning($"Intento de inicio de sesión fallido: Contraseña incorrecta para el usuario {loginDTO.Email}");
                throw new InvalidOperationException("Credenciales invalidas.");

            }

            // Validar si el correo electrónico está verificado
            if (!user.EmailConfirmed)
            {

                Log.Warning($"Intento de inicio de sesión fallido: Correo electrónico no verificado para el usuario {loginDTO.Email}");
                throw new InvalidOperationException("Credenciales invalidas. Por favor, verifica tu correo electrónico antes de iniciar sesión.");
            }

            // Generar token JWT
            string token = _tokenService.GenerateToken(user, user.Role.Name);
            Log.Information($"Token JWT generado para el usuario: {token}");

            return token;
        }

        public async Task<int> DeleteUnconfirmedUsersAsync()
        {
            int deletedUsers = await _userRepository.DeleteUnconfirmedUsersAsync(_daysToDeleteUnverifiedAccount);
            Log.Information($"Usuarios no confirmados eliminados exitosamente. Cantidad: {deletedUsers}");
            return deletedUsers;
        }
    }
}
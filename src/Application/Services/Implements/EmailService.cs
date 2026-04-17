using Resend;
using TiendaUCN.src.Application.Services.Interfaces;

namespace TiendaUCN.src.Application.Services.Implements
{
    public class EmailService : IEmailService
    {
        private readonly IResend _resend;
        private readonly IConfiguration _configuration;

        public EmailService(IResend resend, IConfiguration configuration)
        {
            _resend = resend;
            _configuration = configuration;
        }

        public async Task SendVerificationCodeEmailAsync(string email, string verificationCode)
        {
            var message = new EmailMessage
            {
                To = email,
                Subject = _configuration["EmailConfiguration:VerificationSubject"] ?? throw new ArgumentNullException("El asunto del correo de verificación no está configurado"),
                From = _configuration["EmailConfiguration:From"] ?? throw new ArgumentNullException("La configuración del 'From' no puede ser nula"),
                HtmlBody = $"<p style= 'font-size: 48px; font-weight: bold; letter-spacing: 8px;'>{verificationCode}</p>"
            };

            await _resend.EmailSendAsync(message);
        }

        public async Task SendWelcomeEmailAsync(string email)
        {
            var message = new EmailMessage
            {
                To = email,
                Subject = _configuration["EmailConfiguration:WelcomeSubject"] ?? throw new ArgumentNullException("El asunto del correo de bienvenida no está configurado"),
                From = _configuration["EmailConfiguration:From"] ?? throw new ArgumentNullException("La configuración del 'From' no puede ser nula"),
                HtmlBody = "<p style= 'font-size: 24px; font-weight: bold;'>¡Tu cuenta ha sido verificada exitosamente!</p>"
            };

            await _resend.EmailSendAsync(message);
        }
    }
}
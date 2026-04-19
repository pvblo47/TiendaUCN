namespace TiendaUCN.src.Application.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendVerificationCodeEmailAsync(string email, string verificationCode);
        Task SendWelcomeEmailAsync(string email);
    }
}
using TiendaUCN.src.Domain.Models;

namespace TiendaUCN.src.Infrastructure.Repositories.Interfaces
{
    public interface IVerificationCodeRepository
    {
        Task<VerificationCode> CreateAsync(VerificationCode verificationCode);
        Task<bool> UpdateFailedAttemptsAsync(int id);
        Task<bool> UpdateAsync(int id, string code, DateTime expiry);
        Task<bool> UpdateDateToResendAsync(int id, DateTime dateToResend);
    }
}
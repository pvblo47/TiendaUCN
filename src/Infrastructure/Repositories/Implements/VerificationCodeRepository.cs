using Microsoft.EntityFrameworkCore;
using TiendaUCN.src.Domain.Models;
using TiendaUCN.src.Infrastructure.Data;
using TiendaUCN.src.Infrastructure.Repositories.Interfaces;

namespace TiendaUCN.src.Infrastructure.Repositories.Implements
{
    public class VerificationCodeRepository : IVerificationCodeRepository
    {
        DataContext _context;
        public VerificationCodeRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<VerificationCode> CreateAsync(VerificationCode verificationCode)
        {
            var result = await _context.VerificationCodes.AddAsync(verificationCode);
            await _context.SaveChangesAsync();
            return result.Entity;
        }

        public async Task<bool> UpdateFailedAttemptsAsync(int id)
        {
            // Incrementa el contador de intentos fallidos para el código de verificación con el ID especificado
            var result = await _context.VerificationCodes
                .Where(v => v.Id == id)
               .ExecuteUpdateAsync(v => v.SetProperty(x => x.FailedAttempts, x => x.FailedAttempts + 1));
            return result > 0;
        }

        public async Task<bool> UpdateAsync(int id, string code, DateTime expiry)
        {
            // Actualiza el código de verificación y su fecha de expiración para el código con el ID especificado
            var result = await _context.VerificationCodes
                .Where(v => v.Id == id)
                .ExecuteUpdateAsync(v => v.SetProperty(x => x.Code, code).SetProperty(x => x.Expiry, expiry));
            return result > 0;
        }

        public async Task<bool> UpdateDateToResendAsync(int id, DateTime dateToResend)
        {
            // Actualiza la fecha para reenviar un nuevo código de verificación para el código con el ID especificado
            var result = await _context.VerificationCodes.Where(v => v.Id == id)
                .ExecuteUpdateAsync(v => v.SetProperty(x => x.DateToResend, dateToResend));
            return result > 0;
        }
    }
}
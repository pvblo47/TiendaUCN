using Microsoft.EntityFrameworkCore;
using TiendaUCN.src.Domain.Models;
using TiendaUCN.src.Infrastructure.Data;
using TiendaUCN.src.Infrastructure.Repositories.Interfaces;

namespace TiendaUCN.src.Infrastructure.Repositories.Implements
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;

        public UserRepository(DataContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.Users
                .AnyAsync(u =>
                    u.Name == name &&
                    u.IsDeleted == false);
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _context.Users
                .AnyAsync(u =>
                    u.Email.ToLower() == email.ToLower() &&
                    u.IsDeleted == false);
        }

        public async Task<bool> ExistsByRutAsync(string rut)
        {
            return await _context.Users
                .AnyAsync(u =>
                    u.Rut == rut &&
                    u.IsDeleted == false);
        }

        public async Task<bool> ExistsByPhoneNumberAsync(string phoneNumber)
        {
            return await _context.Users
                .AnyAsync(u =>
                    u.PhoneNumber == phoneNumber
                    && u.IsDeleted == false);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            // Incluir las entidades relacionadas Role y VerificationCode al obtener el usuario por correo electrónico
            return await _context.Users
                .Include(u => u.Role)
                .Include(u => u.VerificationCode)
                .FirstOrDefaultAsync(u =>
                    u.Email.ToLower() == email.ToLower()
                    && u.IsDeleted == false);
        }

        public async Task<bool> MarkEmailAsVerifiedAsync(int id)
        {
            var result = await _context.Users.Where(u => u.Id == id).ExecuteUpdateAsync(u => u.SetProperty(x => x.EmailConfirmed, true));
            return result > 0;
        }

        public async Task<int> DeleteUnconfirmedUsersAsync(int daysToDeleteUnverifiedAccount)
        {
            var now = DateTime.UtcNow;

            // Elimina los códigos de verificación
            // Asociados a los usuarios que cumplen con las condiciones de eliminación
            await _context.VerificationCodes
                .Where(vc =>
                    _context.Users
                        .Any(u =>
                            u.Id == vc.UserId &&
                            u.EmailConfirmed == false &&
                            u.IsDeleted == false &&
                            u.CreatedAt.AddDays(daysToDeleteUnverifiedAccount) <= now))
                .ExecuteDeleteAsync();

            // Elimina los usuarios que:
            // No han confirmado su correo
            // No han sido eliminados previamente
            // Fueron creados hace más de 'daysToDeleteUnverifiedAccount' días
            var result = await _context.Users
                .Where(x =>
                    x.EmailConfirmed == false &&
                    x.IsDeleted == false &&
                    x.CreatedAt.AddDays(daysToDeleteUnverifiedAccount) <= now)
                .ExecuteUpdateAsync(u => u.SetProperty(x => x.IsDeleted, true));

            return result;
        }
    }
}
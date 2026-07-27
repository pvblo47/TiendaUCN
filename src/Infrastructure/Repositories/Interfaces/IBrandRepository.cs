using TiendaUCN.src.Domain.Models;

namespace TiendaUCN.src.Infrastructure.Repositories.Interfaces
{
    public interface IBrandRepository
    {
        Task<bool> ExistsByNameAsync(string name);
        Task<bool> CreateAsync(Brand brand);
        Task<bool> ExistsByIdAsync(int id);
        Task<bool> UpdateNameAsync(int id, string name);
        Task<bool> UpdateDescriptionAsync(int id, string description);
        Task<bool> DeleteAsync(int id);
        Task<int> GetIdByNameAsync(string name);
        Task<List<Brand>> GetAllActiveAsync();
    }
}
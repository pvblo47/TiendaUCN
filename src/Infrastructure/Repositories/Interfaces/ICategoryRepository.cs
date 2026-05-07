using TiendaUCN.src.Domain.Models;

namespace TiendaUCN.src.Infrastructure.Repositories.Interfaces
{
    public interface ICategoryRepository
    {
        Task<bool> ExistsByNameAsync(string name);
        Task<bool> CreateAsync(Category category);
        Task<bool> ExistsByIdAsync(int id);
        Task<bool> UpdateNameAsync(int id, string name);
        Task<bool> UpdateDescriptionAsync(int id, string description);
        Task<bool> DeleteAsync(int id);
        Task<int> GetIdByNameAsync(string name);
    }
}
using TiendaUCN.src.Domain.Models;

namespace TiendaUCN.src.Infrastructure.Repositories.Interfaces
{
    public interface IImageRepository
    {
        Task<bool?> CreateAsync(Image image);
        Task<bool?> DeleteAsync(string publicId);
    }
}

using TiendaUCN.src.Application.DTOs.BrandCategoryDTO;

namespace TiendaUCN.src.Application.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<string> CreateCategoryAsync(CreateBrandCategoryDTO createCategoryDTO);
        Task<string> UpdateCategoryAsync(int id, UpdateBrandCategoryDTO updateCategoryDTO);
        Task<string> DeleteCategoryAsync(int id);
    }
}
using TiendaUCN.src.Application.DTOs.BrandCategoryDTO;

namespace TiendaUCN.src.Application.Services.Interfaces
{
    public interface IBrandService
    {
        Task<string> CreateBrandAsync(CreateBrandCategoryDTO brandDto);
        Task<string> UpdateBrandAsync(int brandId, UpdateBrandCategoryDTO brandDto);
        Task<string> DeleteBrandAsync(int brandId);
    }
}
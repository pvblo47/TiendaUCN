using CloudinaryDotNet;
using TiendaUCN.src.Application.DTOs.ProductDTO;
using TiendaUCN.src.Application.DTOs.ProductDTO.Admin;
using TiendaUCN.src.Domain.Models;

namespace TiendaUCN.src.Infrastructure.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task<bool> ExistsByNameAndBrandAsync(string name, string brandName);
        Task<bool> CreateAsync(Product product);
        Task<bool> ExistsByIdAsync(int id);
        Task<bool> SwitchStatusAsync(int id);
        Task<string?> GetStatusAsync(int id);
        Task<bool> ExistsByIdCustomerAsync(int id);
        Task<Product?> GetProductByIdForCustomerAsync(int id);
        Task<Product?> GetProductByIdForAdminAsync(int id);
        Task<bool> DeleteAsync(int id);
        Task<(IEnumerable<Product> products, int totalCount)> GetFilteredForAdminAsync(SearchParamsDTO searchParams);
        Task<(IEnumerable<Product> products, int totalCount)> GetFilteredForCustomerAsync(SearchParamsDTO searchParams);
        Task<bool> UpdateAsync(Product product);

        /// <summary>
        /// Actualiza el stock de un producto por su ID.
        /// </summary>
        /// <param name="productId">El ID del producto</param>
        /// <param name="newStock">El nuevo stock</param>
        /// <returns>true si lo actualiza, false si no</returns>
        Task<bool> UpdateStockAsync(int productId, int newStock);
    }
}
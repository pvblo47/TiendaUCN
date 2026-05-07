using TiendaUCN.src.Application.DTOs.ProductDTO;
using TiendaUCN.src.Application.DTOs.ProductDTO.Admin;
using TiendaUCN.src.Application.DTOs.ProductDTO.Customer;

namespace TiendaUCN.src.Application.Services.Interfaces
{
    public interface IProductService
    {
        Task<string> CreateProductAsync(CreateProductDTO createProductDTO);
        Task<string> SwitchStatusProductAsync(int id);
        Task<ProductDetailCustomerDTO> GetProductByIdForCustomerAsync(int id);
        Task<ProductDetailAdminDTO> GetProductByIdForAdminAsync(int id);
        Task DeleteProductAsync(int id);
        Task<ListedProductsForAdminDTO> GetListedProductsForAdminAsync(SearchParamsDTO searchParams);
        Task<ListedProductsForCustomerDTO> GetListedProductsForCustomerAsync(SearchParamsDTO searchParams);
        Task UpdateProductAsync(int id, UpdateProductDTO updateProductDTO);
    }
}
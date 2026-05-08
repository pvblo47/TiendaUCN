using TiendaUCN.src.Application.DTOs.CartDTO;

namespace TiendaUCN.src.Application.Services.Interfaces
{
    public interface ICartService
    {
        Task<CartDTO> CreateOrGetCartAsync(string buyerId, int? userId = null);
        Task<CartDTO> AddCartItemAsync(string buyerId, AddChangeCartItemDTO addCartItemDTO, int? userId = null);
        Task<CartDTO> UpdateCartItemQuantityAsync(string buyerId, AddChangeCartItemDTO changeCartItemDTO, int? userId = null);
        Task<CartDTO> RemoveCartItemAsync(string buyerId, int productId, int? userId = null);
        Task<CartDTO> ClearCartAsync(string buyerId, int? userId = null);

        Task<CheckoutResultDTO> CheckoutCartAsync(int userId);
    }
}
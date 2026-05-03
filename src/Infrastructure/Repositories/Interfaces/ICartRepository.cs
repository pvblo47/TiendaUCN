using TiendaUCN.src.Domain.Models;

namespace TiendaUCN.src.Infrastructure.Repositories.Interfaces
{
    public interface ICartRepository
    {
        Task<Cart?> GetByUserIdAsync(int userId);
        Task<Cart?> GetByBuyerIdAsync(string buyerId);
        Task<bool> CreateAsync(Cart cart);
        Task<bool> AddItemAsync(CartItem cartItem);
        Task UpdateItemQuantityAsync(int cartId, int cartItemId, int newQuantity);
        Task UpdateTotalPriceAsync(int cartId, int newTotalPrice);
        Task<bool> RemoveItemAsync(int cartId, int cartItemId);
        Task<bool> ClearCartItemsAsync(int cartId);
    }
}
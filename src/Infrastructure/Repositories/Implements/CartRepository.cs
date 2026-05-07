using Microsoft.EntityFrameworkCore;
using TiendaUCN.src.Domain.Models;
using TiendaUCN.src.Infrastructure.Data;
using TiendaUCN.src.Infrastructure.Repositories.Interfaces;

namespace TiendaUCN.src.Infrastructure.Repositories.Implements
{
    public class CartRepository : ICartRepository
    {
        private readonly DataContext _context;
        public CartRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<Cart?> GetByUserIdAsync(int userId)
        {
            return await _context.Carts
                .AsNoTracking() // Solo lectura, no guarda el cart en cache
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                        .ThenInclude(p => p.Images)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<Cart?> GetByBuyerIdAsync(string buyerId)
        {
            // Retornar el carrito exclusivo para el navegador
            return await _context.Carts
                .AsNoTracking() // Solo lectura, no guarda el cart en cache
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                        .ThenInclude(p => p.Images)
                .FirstOrDefaultAsync(c => c.BuyerId == buyerId && c.UserId == null);
        }

        public async Task<bool> CreateAsync(Cart cart)
        {
            _context.Carts.Add(cart);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> AddItemAsync(CartItem cartItem)
        {
            _context.CartItems.Add(cartItem);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task UpdateItemQuantityAsync(int cartId, int cartItemId, int newQuantity)
        {
            await _context.CartItems
                .Where(ci => ci.Id == cartItemId && ci.CartId == cartId)
                .ExecuteUpdateAsync(ci =>
                    ci.SetProperty(c => c.Quantity, newQuantity));
        }

        public async Task UpdateTotalPriceAsync(int cartId, int newTotalPrice)
        {
            await _context.Carts
                .Where(c => c.Id == cartId)
                .ExecuteUpdateAsync(c =>
                    c.SetProperty(c => c.TotalPrice, newTotalPrice));
        }

        public async Task<bool> RemoveItemAsync(int cartId, int cartItemId)
        {
            return await _context.CartItems
                .Where(ci => ci.Id == cartItemId && ci.CartId == cartId)
                .ExecuteDeleteAsync() > 0;
        }

        public async Task<bool> ClearCartItemsAsync(int cartId)
        {
            return await _context.CartItems
                .Where(ci => ci.CartId == cartId)
                .ExecuteDeleteAsync() > 0;
        }
    }
}
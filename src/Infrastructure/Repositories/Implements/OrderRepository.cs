using Microsoft.EntityFrameworkCore;
using TiendaUCN.src.Application.DTOs.ProductDTO;
using TiendaUCN.src.Domain.Models;
using TiendaUCN.src.Infrastructure.Data;
using TiendaUCN.src.Infrastructure.Repositories.Interfaces;


namespace TiendaUCN.src.Infrastructure.Repositories.Implements
{
    /// <summary>
    /// Implementación del repositorio de órdenes.
    /// </summary>
    public class OrderRepository : IOrderRepository
    {
        /// <summary>
        /// Contexto de datos para acceder a la base de datos.
        /// </summary>
        private readonly DataContext _context;

        /// <summary>
        /// Constructor del repositorio de órdenes.
        /// </summary>
        /// <param name="context">Contexto de datos para acceder a la base de datos</param>
        public OrderRepository(DataContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Verifica si una orden existe por su código.
        /// </summary>
        /// <param name="code">El código de la orden</param>
        /// <returns>true si existe, false si no</returns>
        public async Task<bool> ExistsByCodeAsync(string code)
        {
            return await _context.Orders.AnyAsync(o => o.Code == code);
        }

        /// <summary>
        /// Crea una nueva orden en la base de datos.
        /// </summary>
        /// <param name="order">La orden a crear</param>
        /// <returns>true si la crea, false si no</returns>
        public async Task<bool> CreateAsync(Order order)
        {
            _context.Orders.Add(order);
            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Obtiene una orden por su código y el ID del usuario.
        /// </summary>
        /// <param name="code">El código de la orden</param>
        /// <param name="userId">El ID del usuario</param>
        /// <returns>La orden encontrada o null si no se encuentra</returns>
        public async Task<Order?> GetByCodeAsync(string code, int userId)
        {
            return await _context.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Code == code && o.UserId == userId);
        }

        /// <summary>
        /// Obtiene un listado de órdenes filtradas por el ID del usuario.
        /// </summary>
        /// <param name="searchParams">Los parámetros de búsqueda</param>
        /// <param name="userId">El ID del usuario</param>
        /// <returns>Una tupla con las órdenes y el total de registros</returns>
        public async Task<(IEnumerable<Order> orders, int totalCount)> GetFilteredForUserIdAsync(SearchParamsDTO searchParams, int userId)
        {
            // Obtener queryable de productos que estén activos y no eliminados
            var query = _context.Orders
                .Include(p => p.OrderItems)
                .Where(p => p.UserId == userId)
                .AsNoTracking(); // Para mejorar el rendimiento en consultas de solo lectura

            // Aplicar filtro de búsqueda si se proporciona un término de búsqueda
            if (!string.IsNullOrWhiteSpace(searchParams.SearchTerm))
            {
                var searchTerm = searchParams.SearchTerm.Trim().ToLower();
                query = query.Where(p =>
                    p.Code.Contains(searchTerm) ||
                    p.TotalPrice.ToString().Contains(searchTerm) ||
                    p.OrderItems.Any(oi => oi.Quantity.ToString().Contains(searchTerm)) ||
                    p.OrderItems.Any(oi => oi.NameAtMoment.ToLower().Contains(searchTerm)) ||
                    p.OrderItems.Any(oi => oi.DescriptionAtMoment.ToLower().Contains(searchTerm)) ||
                    p.OrderItems.Any(oi => oi.UnitPriceAtMoment.ToString().Contains(searchTerm)) ||
                    p.OrderItems.Any(oi => oi.SubtotalPrice.ToString().Contains(searchTerm)));
            }

            // Obtener el total de orders que cumplen con el filtro
            int totalCount = await query.CountAsync();

            // Aplicar paginación
            var orders = await query
                .OrderByDescending(p => p.TransactionDate)
                .Skip((searchParams.PageNumber - 1) * searchParams.PageSize)
                .Take(searchParams.PageSize)
                .ToArrayAsync();

            return (orders, totalCount);
        }
    }
}
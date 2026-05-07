using TiendaUCN.src.Application.DTOs.ProductDTO;
using TiendaUCN.src.Domain.Models;

namespace TiendaUCN.src.Infrastructure.Repositories.Interfaces
{
    /// <summary>
    /// Interfaz para el repositorio de órdenes.
    /// </summary>
    public interface IOrderRepository
    {
        /// <summary>
        /// Verifica si una orden existe por su código.
        /// </summary>
        /// <param name="code">El código de la orden</param>
        /// <returns>true si existe, false si no</returns>
        Task<bool> ExistsByCodeAsync(string code);

        /// <summary>
        /// Crea una nueva orden en la base de datos.
        /// </summary>
        /// <param name="order">La orden a crear</param>
        /// <returns>true si la crea, false si no</returns>
        Task<bool> CreateAsync(Order order);

        /// <summary>
        /// Obtiene una orden por su código y el ID del usuario.
        /// </summary>
        /// <param name="code">El código de la orden</param>
        /// <param name="userId">El ID del usuario</param>
        /// <returns>La orden encontrada o null si no se encuentra</returns>
        Task<Order?> GetByCodeAsync(string code, int userId);

        /// <summary>
        /// Obtiene un listado de órdenes filtradas por el ID del usuario.
        /// </summary>
        /// <param name="searchParams">Los parámetros de búsqueda</param>
        /// <param name="userId">El ID del usuario</param>
        /// <returns>Una tupla con las órdenes y el total de registros</returns>
        Task<(IEnumerable<Order> orders, int totalCount)> GetFilteredForUserIdAsync(SearchParamsDTO searchParams, int userId);
    }
}
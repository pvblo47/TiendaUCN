using TiendaUCN.src.Application.DTOs.OrderDTO;
using TiendaUCN.src.Application.DTOs.ProductDTO;

namespace TiendaUCN.src.Application.Services.Interfaces
{
    /// <summary>
    /// Interfaz del servicio de órdenes.
    /// </summary>
    public interface IOrderService
    {
        /// <summary>
        /// Crea una nueva orden para un usuario.
        /// </summary>
        /// <param name="userId">Identificador del usuario.</param>
        /// <returns>Código de la orden creada.</returns>
        Task<string> CreateOrderAsync(int userId);

        /// <summary>
        /// Obtiene el detalle de una orden.
        /// </summary>
        /// <param name="orderCode">Código de la orden.</param>
        /// <param name="userId">Identificador del usuario.</param>
        /// <returns>Detalle de la orden.</returns>
        Task<OrderDetailDTO> GetOrderDetailAsync(string orderCode, int userId);

        /// <summary>
        /// Obtiene las órdenes de un usuario.
        /// </summary>
        /// <param name="searchParams">Parámetros de búsqueda y paginación.</param>
        /// <param name="userId">Identificador del usuario.</param>
        /// <returns>Listado de órdenes.</returns>
        Task<ListedOrderDetailDTO> GetOrdersByUserIdAsync(SearchParamsDTO searchParams, int userId);
    }
}
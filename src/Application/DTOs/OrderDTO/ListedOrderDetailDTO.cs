namespace TiendaUCN.src.Application.DTOs.OrderDTO
{
    /// <summary>
    /// DTO que representa un listado paginado de órdenes con sus detalles.
    /// </summary>
    public class ListedOrderDetailDTO
    {
        /// <summary>
        /// Lista de órdenes con sus respectivos detalles.
        /// </summary>
        public required List<OrderDetailDTO> Orders { get; set; } = new List<OrderDetailDTO>();

        /// <summary>
        /// Cantidad total de órdenes disponibles.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Cantidad total de páginas disponibles.
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Página actual del listado.
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// Cantidad de elementos por página.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Cantidad de órdenes presentes en la página actual.
        /// </summary>
        public int OrdersInPage { get; set; }
    }
}
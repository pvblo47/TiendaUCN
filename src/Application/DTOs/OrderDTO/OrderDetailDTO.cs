namespace TiendaUCN.src.Application.DTOs.OrderDTO
{
    /// <summary>
    /// DTO que representa el detalle de una orden.
    /// </summary>
    public class OrderDetailDTO
    {
        /// <summary>
        /// Código identificador de la orden.
        /// </summary>
        public required string Code { get; set; }

        /// <summary>
        /// Fecha y hora en que se realizó la transacción.
        /// </summary>
        public required DateTime TransactionDate { get; set; }

        /// <summary>
        /// Precio total de la orden.
        /// </summary>
        public required string TotalPrice { get; set; }

        /// <summary>
        /// Lista de ítems asociados a la orden.
        /// </summary>
        public required List<OrderItemDTO> Items { get; set; }
    }
}
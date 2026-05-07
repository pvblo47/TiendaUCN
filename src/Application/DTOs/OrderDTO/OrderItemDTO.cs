namespace TiendaUCN.src.Application.DTOs.OrderDTO
{
    /// <summary>
    /// DTO que representa un ítem dentro de una orden.
    /// </summary>
    public class OrderItemDTO
    {
        /// <summary>
        /// Nombre del producto.
        /// </summary>
        public required string ProductName { get; set; }

        /// <summary>
        /// Descripción del producto.
        /// </summary>
        public required string ProductDescription { get; set; }

        /// <summary>
        /// URL de la imagen principal del producto.
        /// </summary>
        public required string MainImageURL { get; set; }

        /// <summary>
        /// Precio unitario del producto al momento de la compra.
        /// </summary>
        public required string UnitPriceAtMoment { get; set; }

        /// <summary>
        /// Precio subtotal correspondiente a la cantidad del producto.
        /// </summary>
        public required string SubtotalPrice { get; set; }

        /// <summary>
        /// Cantidad del producto en la orden.
        /// </summary>
        public required int Quantity { get; set; }
    }
}
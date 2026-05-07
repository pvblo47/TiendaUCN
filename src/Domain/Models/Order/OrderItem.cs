namespace TiendaUCN.src.Domain.Models
{
    /// <summary>
    /// Entidad de ítem de pedido.
    /// </summary>
    public class OrderItem
    {
        /// <summary>
        /// Identificador del ítem.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Cantidad del producto.
        /// </summary>
        public required int Quantity { get; set; }

        /// <summary>
        /// Nombre del producto al momento de la compra.
        /// </summary>
        public required string NameAtMoment { get; set; }

        /// <summary>
        /// Descripción del producto al momento de la compra.
        /// </summary>
        public required string DescriptionAtMoment { get; set; }

        /// <summary>
        /// Precio unitario al momento de la compra.
        /// </summary>
        public required int UnitPriceAtMoment { get; set; }

        /// <summary>
        /// URL de la imagen al momento de la compra.
        /// </summary>
        public required string ImageUrlAtMoment { get; set; }

        /// <summary>
        /// Precio subtotal del ítem.
        /// </summary>
        public required int SubtotalPrice { get; set; }

        /// <summary>
        /// Identificador del pedido.
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// Pedido asociado al ítem.
        /// </summary>
        public Order Order { get; set; } = null!;
    }
}
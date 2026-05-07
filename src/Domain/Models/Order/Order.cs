namespace TiendaUCN.src.Domain.Models
{
    /// <summary>
    /// Entidad de pedido.
    /// </summary>
    public class Order
    {
        /// <summary>
        /// Identificador del pedido.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Código del pedido.
        /// </summary>
        public required string Code { get; set; }

        /// <summary>
        /// Fecha de la transacción.
        /// </summary>
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Precio total del pedido.
        /// </summary>
        public required int TotalPrice { get; set; }

        /// <summary>
        /// Identificador del usuario.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Usuario asociado al pedido.
        /// </summary>
        public User User { get; set; } = null!;

        /// <summary>
        /// Lista de ítems del pedido.
        /// </summary>
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
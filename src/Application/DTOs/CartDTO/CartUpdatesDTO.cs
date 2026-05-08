namespace TiendaUCN.src.Application.DTOs.CartDTO
{
    /// <summary>
    /// DTO que representa los cambios realizados en el carrito de compras.
    /// </summary>
    public class CartUpdatesDTO
    {
        /// <summary>
        /// Lista de nombres de los ítems que fueron actualizados en el carrito.
        /// </summary>
        public List<string> UpdatedItemsNames { get; set; } = null!;

        /// <summary>
        /// Lista de nombres de los ítems que fueron eliminados del carrito.
        /// </summary>
        public List<string> RemovedItemsNames { get; set; } = null!;
    }
}
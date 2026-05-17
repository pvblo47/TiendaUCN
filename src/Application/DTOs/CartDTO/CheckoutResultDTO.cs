using TiendaUCN.src.Domain.Models;

namespace TiendaUCN.src.Application.DTOs.CartDTO
{
    /// <summary>
    /// DTO que representa el resultado del proceso de checkout.
    /// </summary>
    public class CheckoutResultDTO
    {
        /// <summary>
        /// Carrito actualizado después del proceso de checkout.
        /// </summary>
        public required CartDTO CartUpdated { get; set; }

        /// <summary>
        /// Información sobre los cambios realizados en el carrito durante el checkout.
        /// </summary>
        public CartUpdatesDTO CartUpdatesDTO { get; set; } = null!;
    }
}
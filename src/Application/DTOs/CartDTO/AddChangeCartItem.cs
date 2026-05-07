using System.ComponentModel.DataAnnotations;

namespace TiendaUCN.src.Application.DTOs.CartDTO
{
    public class AddChangeCartItemDTO
    {
        [Required(ErrorMessage = "El ID del producto es requerido.")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del producto debe ser un número positivo.")]
        public required int ProductId { get; set; }

        [Required(ErrorMessage = "La cantidad es requerida.")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser un número positivo.")]
        public required int Quantity { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace TiendaUCN.src.Application.DTOs.ProductDTO
{
    public class SearchParamsDTO
    {
        [Required(ErrorMessage = "El número de página es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El número de página debe ser un número entero positivo.")]
        public required int PageNumber { get; set; }

        [Required(ErrorMessage = "El tamaño de página es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El tamaño de página debe ser un número entero positivo.")]
        public required int PageSize { get; set; }

        [MinLength(2, ErrorMessage = "El término de búsqueda debe tener al menos 2 caracteres.")]
        [MaxLength(40, ErrorMessage = "El término de búsqueda no puede exceder los 40 caracteres.")]
        public string? SearchTerm { get; set; }
    }
}
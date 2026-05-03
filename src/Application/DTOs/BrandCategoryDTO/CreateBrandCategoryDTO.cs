using System.ComponentModel.DataAnnotations;

namespace TiendaUCN.src.Application.DTOs.BrandCategoryDTO
{
    public class CreateBrandCategoryDTO
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MinLength(3, ErrorMessage = "El nombre debe tener mínimo 3 letras.")]
        [MaxLength(25, ErrorMessage = "El nombre debe tener máximo 25 letras.")]
        public required string Name { get; set; }

        [MinLength(3, ErrorMessage = "La descripción debe tener mínimo 3 letras.")]
        [MaxLength(250, ErrorMessage = "La descripción debe tener máximo 250 letras.")]
        public string? Description { get; set; }
    }
}
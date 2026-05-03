using System.ComponentModel.DataAnnotations;

namespace TiendaUCN.src.Application.DTOs.ProductDTO.Admin
{
    public class UpdateProductDTO
    {
        [StringLength(20, ErrorMessage = "El nombre no puede exceder los 20 caracteres.")]
        [MinLength(3, ErrorMessage = "El nombre debe tener al menos 3 caracteres.")]
        public string? Name { get; set; }

        [StringLength(100, ErrorMessage = "La descripción no puede exceder los 100 caracteres.")]
        [MinLength(10, ErrorMessage = "La descripción debe tener al menos 10 caracteres.")]
        public string? Description { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El precio debe ser un valor entero positivo mayor que cero.")]
        public int? Price { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El stock debe ser un valor entero positivo mayor que cero.")]
        public int? Stock { get; set; }

        [StringLength(25, ErrorMessage = "El nombre de la categoría no puede exceder los 25 caracteres.")]
        [MinLength(3, ErrorMessage = "El nombre de la categoría debe tener al menos 3 caracteres.")]
        public string? CategoryName { get; set; }

        [StringLength(25, ErrorMessage = "El nombre de la marca no puede exceder los 25 caracteres.")]
        [MinLength(3, ErrorMessage = "El nombre de la marca debe tener al menos 3 caracteres.")]
        public string? BrandName { get; set; }

        public List<string>? ImagesToRemove { get; set; } // Lista de URLs de imágenes a eliminar

        public List<IFormFile>? ImagesToAdd { get; set; } // Nuevas imágenes a agregar
    }
}
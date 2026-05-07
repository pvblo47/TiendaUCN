using System.ComponentModel.DataAnnotations;

namespace TiendaUCN.src.Application.DTOs.ProductDTO
{
    public class CreateProductDTO
    {
        [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
        [StringLength(20, ErrorMessage = "El nombre no puede exceder los 20 caracteres.")]
        [MinLength(3, ErrorMessage = "El nombre debe tener al menos 3 caracteres.")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "La descripción del producto es obligatoria.")]
        [StringLength(100, ErrorMessage = "La descripción no puede exceder los 100 caracteres.")]
        [MinLength(10, ErrorMessage = "La descripción debe tener al menos 10 caracteres.")]
        public required string Description { get; set; }

        [Required(ErrorMessage = "El precio del producto es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El precio debe ser un valor entero positivo mayor que cero.")]
        public required int Price { get; set; }

        [Required(ErrorMessage = "El stock del producto es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El stock debe ser un valor entero positivo mayor que cero.")]
        public required int Stock { get; set; }

        [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
        [StringLength(25, ErrorMessage = "El nombre de la categoría no puede exceder los 25 caracteres.")]
        [MinLength(3, ErrorMessage = "El nombre de la categoría debe tener al menos 3 caracteres.")]
        public required string CategoryName { get; set; }

        [Required(ErrorMessage = "El nombre de la marca es obligatorio.")]
        [StringLength(25, ErrorMessage = "El nombre de la marca no puede exceder los 25 caracteres.")]
        [MinLength(3, ErrorMessage = "El nombre de la marca debe tener al menos 3 caracteres.")]
        public required string BrandName { get; set; }

        [Required(ErrorMessage = "Las imágenes del producto son obligatorias.")]
        [MinLength(1, ErrorMessage = "Debe proporcionar al menos una imagen para el producto.")]
        public required List<IFormFile> ImagesFiles { get; set; }
    }
}
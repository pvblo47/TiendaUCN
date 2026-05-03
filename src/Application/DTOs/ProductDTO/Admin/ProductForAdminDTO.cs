namespace TiendaUCN.src.Application.DTOs.ProductDTO.Admin
{
    public class ProductForAdminDTO
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public string? MainImageURL { get; set; }
        public required string Price { get; set; }
        public required int Stock { get; set; }
        public required string Available { get; set; }
    }
}
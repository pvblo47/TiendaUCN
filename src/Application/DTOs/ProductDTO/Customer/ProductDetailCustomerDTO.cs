namespace TiendaUCN.src.Application.DTOs.ProductDTO
{
    public class ProductDetailCustomerDTO
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required string Price { get; set; }
        public required string StockIndicator { get; set; }
        public required string BrandName { get; set; }
        public required string BrandDescription { get; set; }
        public required string CategoryName { get; set; }
        public required string CategoryDescription { get; set; }
        public List<string> ImagesURL { get; set; } = new List<string>();
    }
}
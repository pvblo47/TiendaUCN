namespace TiendaUCN.src.Application.DTOs.ProductDTO.Customer
{
    public class ProductForCustomerDTO
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string MainImageURL { get; set; }
        public required string Price { get; set; }
        public required string StockIndicator { get; set; }
    }
}
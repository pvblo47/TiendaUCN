namespace TiendaUCN.src.Application.DTOs.CartDTO
{
    public class CartItemDTO
    {
        public required int ProductId { get; set; }
        public required string ProductName { get; set; }
        public required string ProductImageUrl { get; set; }
        public required string ProductPrice { get; set; }
        public required int Quantity { get; set; }
        public required string TotalPrice { get; set; }
    }
}
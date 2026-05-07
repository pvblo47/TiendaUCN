namespace TiendaUCN.src.Domain.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public required int Quantity { get; set; }
        public int CartId { get; set; } // Establece la relación con Cart (Un carrito puede tener muchos CartItems)
        public Cart Cart { get; set; } = null!;
        public int ProductId { get; set; } // Establece la relación con Product (Un producto puede estar en muchos CartItems)
        public Product Product { get; set; } = null!;
    }
}
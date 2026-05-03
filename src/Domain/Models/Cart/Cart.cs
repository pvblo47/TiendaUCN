namespace TiendaUCN.src.Domain.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public int TotalPrice { get; set; } = 0;
        public string BuyerId { get; set; } = null!; // Id asociada a un usuario no autenticado
        public int? UserId { get; set; } // Establece la relación con User (Un usuario tiene un carrito)
        public User User { get; set; } = null!;
        public List<CartItem> CartItems { get; set; } = new List<CartItem>(); // Relación con CartItem (Un carrito puede tener muchos CartItems)
    }
}
namespace TiendaUCN.src.Domain.Models
{
    public class Category
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public ICollection<Product> Products { get; set; } = new List<Product>(); // Atributo de referencia a los productos
        public bool IsDeleted { get; set; } = false;
    }
}
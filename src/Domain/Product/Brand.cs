namespace TiendaUCN.src.Domain.Models
{
    public class Brand
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
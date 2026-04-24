namespace TiendaUCN.src.Domain.Models
{
    public class Role
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}

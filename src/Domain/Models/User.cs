namespace TiendaUCN.src.Domain.Models
{
    public class User
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public bool EmailConfirmed { get; set; } = false;
        public required string Rut { get; set; }
        public required string PhoneNumber { get; set; }
        public required DateTime BirthDate { get; set; }
        public required string Gender { get; set; }
        public required string PasswordHash { get; set; }
        public int RoleId { get; set; } = 2; // Establece la relación con Role (Un rol puede tener muchos usuarios)
        public Role Role { get; set; } = null!;
        public VerificationCode VerificationCode { get; set; } = null!; // Navegación opcional a VerificationCode
        //public Cart Cart { get; set; } = null!;  // Referencia opcional a Cart
        //public ICollection<Order> Orders { get; set; } = new List<Order>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
    }
}
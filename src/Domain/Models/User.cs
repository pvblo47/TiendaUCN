using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TiendaUCN.src.Domain.Models
{
    public class User
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public bool EmailConfirmed { get; set; } = false;
        public string? VerificationCode { get; set; }
        public DateTime? VerificationCodeExpiry { get; set; }
        public required string Rut { get; set; }
        public required string PhoneNumber { get; set; }
        public required DateTime BirthDate { get; set; }
        public required string Gender { get; set; }
        public required string PasswordHash { get; set; }
        public int RoleId { get; set; } // Establece relación con Role (un rol que puede tener muchos usuarios)
        public Role Role { get; set; } = null!;
        public bool IsDeleted { get; set; } = false;
    }
}
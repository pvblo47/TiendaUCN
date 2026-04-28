using System.ComponentModel.DataAnnotations;

namespace TiendaUCN.src.Application.DTOs.AuthDTO
{
    public class ResendVerificationCodeDTO
    {
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
        public required string Email { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;
using TiendaUCN.src.Application.Validators;

namespace TiendaUCN.src.Application.DTOs.AuthDTO
{
    public class RegisterDTO
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MinLength(2, ErrorMessage = "El nombre debe tener mínimo 2 letras.")]
        [MaxLength(20, ErrorMessage = "El nombre debe tener máximo 20 letras.")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ\s\-]+$", ErrorMessage = "El Nombre solo puede contener carácteres del abecedario español")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "El RUT es obligatorio.")]
        [RegularExpression(@"^\d{7,8}-[0-9kK]$", ErrorMessage = "El Rut debe tener formato XXXXXXXX-X")]
        [RutValidation(ErrorMessage = "El Rut no es válido.")]
        public required string Rut { get; set; }

        [Required(ErrorMessage = "El número de teléfono es obligatorio.")]
        [RegularExpression(@"^\+569\s\d{8}$", ErrorMessage = "El número de teléfono debe tener el formato +569 xxxxxxxx. ")]
        public required string PhoneNumber { get; set; }

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
        [BirthDateValidation]
        public required DateTime BirthDate { get; set; }

        [Required(ErrorMessage = "El género es obligatorio.")]
        [RegularExpression(@"^(Masculino|Femenino|otro)$", ErrorMessage = "El género debe ser Masculino, Femenino u Otro.")]

        public required string Gender { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[0-9])(?=.*[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ])(?=.*[!@#$%^&*()+\[\]{};:'""\\|,.<>/?]).*$",
            ErrorMessage = "La contraseña debe ser alfanumérica, contener al menos una mayúscula y al menos un caracter especial.")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        [MaxLength(20, ErrorMessage = "La contraseña debe tener como máximo 20 caracteres")]

        public required string Password { get; set; }

        [Required(ErrorMessage = "La confirmación de contraseña es obligatoria.")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        public required string ConfirmPassword { get; set; }
    }
}


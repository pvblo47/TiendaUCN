namespace TiendaUCN.src.Domain.Models
{
    public class VerificationCode
    {
        public int Id { get; set; }
        public required string Code { get; set; }
        public required DateTime Expiry { get; set; } // Fecha de expiración del código de verificación
        public int FailedAttempts { get; set; } = 0; // Contador de intentos fallidos de verificación de correo
        public DateTime DateToResend { get; set; } = DateTime.UtcNow; // Fecha para poder reenviar un nuevo código de verificación 
        public int UserId { get; set; } // Establece la relación con User (Un usuario tiene un código de verificación)
    }
}
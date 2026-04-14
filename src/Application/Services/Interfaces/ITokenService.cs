namespace TiendaUCN.src.Application.Services.Interfaces
{
    /// <summary>
    /// Interfaz provisional para el servicio de tokens.
    /// TODO: Definir los métodos según los requerimientos del proyecto.
    /// </summary>
    public interface ITokenService
    {
        Task<string> GenerateTokenAsync(int userId);
        Task<bool> ValidateTokenAsync(string token);
    }
}

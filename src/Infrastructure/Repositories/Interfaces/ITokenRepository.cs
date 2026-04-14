namespace TiendaUCN.src.Infrastructure.Repositories.Interfaces
{
    /// <summary>
    /// Interfaz provisional para el repositorio de tokens.
    /// TODO: Definir los métodos según los requerimientos del proyecto.
    /// </summary>
    public interface ITokenRepository
    {
        Task SaveTokenAsync(int userId, string token);
        Task<bool> IsTokenValidAsync(string token);
        Task RevokeTokenAsync(string token);
    }
}

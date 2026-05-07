namespace TiendaUCN.src.Application.Jobs.Interfaces
{
    public interface IUserJob
    {
        Task DeleteUnconfirmedUsersAsync();
        Task DeleteExpiredTokensInBlacklistAsync();
    }
}
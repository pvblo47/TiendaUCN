namespace TiendaUCN.src.Domain.Models
{
    public class BlackListedToken
    {
        public int Id { get; set; }
        public required string TokenId { get; set; }
        public required DateTime ExpireAt { get; set; }
    }
}
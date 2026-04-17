using Mapster;
using TiendaUCN.src.Application.DTOs.AuthDTO;
using TiendaUCN.src.Domain.Models;

namespace TiendaUCN.src.Application.Mappers
{
    public class UserMapper
    {
        public void ConfigureAllMappings()
        {
            ConfigureAuthMappings();
        }

        private void ConfigureAuthMappings()
        {
            TypeAdapterConfig<RegisterDTO, User>.NewConfig()
                .Map(dest => dest.EmailConfirmed, src => false)
                .Map(dest => dest.PasswordHash, src => BCrypt.Net.BCrypt.HashPassword(src.Password)) // Encriptar la contraseña
                .Map(dest => dest.RoleId, src => 2) // Asignar el rol de cliente por defecto
                .Map(dest => dest.IsDeleted, src => false);
        }
    }
}



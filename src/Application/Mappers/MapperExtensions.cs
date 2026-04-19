using Mapster;
using TiendaUCN.src.Application.Mappers;

namespace TiendaUCN.src.Application.Mappers
{
    public class MapperExtensions
    {
        public static void ConfigureMapster(IServiceProvider serviceProvider)
        {
            // Configuración global de Mapster para ignorar valores nulos
            TypeAdapterConfig.GlobalSettings.Default.IgnoreNullValues(true);

            // Configuración de mapeos específicos
            var userMapper = serviceProvider.GetRequiredService<UserMapper>();
            userMapper.ConfigureAllMappings();
        }
    }
}


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

            //Configuracion de mapeo para usuarios
            var userMapper = serviceProvider.GetRequiredService<UserMapper>();
            userMapper.ConfigureAllMappings();

            //Configuracion de mapeo para productos
            var productMapper = serviceProvider.GetRequiredService<ProductMapper>();
            productMapper.ConfigureAllMappings();

            //Configuracion de mapeo para carritos
            var cartMapper = serviceProvider.GetRequiredService<CartMapper>();
            cartMapper.ConfigureAllMappings();
        }
    }
}


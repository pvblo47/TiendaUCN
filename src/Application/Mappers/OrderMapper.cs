using Mapster;
using TiendaUCN.src.Application.DTOs.OrderDTO;
using TiendaUCN.src.Domain.Models;

namespace TiendaUCN.src.Application.Mappers
{
    /// <summary>
    /// Mapper para las órdenes de compra
    /// </summary>
    public class OrderMapper
    {
        /// <summary>
        /// Interfaz de configuración.
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// URL de la imagen por defecto.
        /// </summary>
        private readonly string _defaultImageUrl;

        /// <summary>
        /// Información de la zona horaria.
        /// </summary>
        private readonly TimeZoneInfo _timeZoneInfo;

        /// <summary>
        /// Constructor del OrderMapper
        /// </summary>
        /// <param name="configuration">Interfaz de configuración</param>
        /// <exception cref="InvalidOperationException"></exception>
        public OrderMapper(IConfiguration configuration)
        {
            _configuration = configuration;
            _defaultImageUrl = _configuration["Products:DefaultImageUrl"] ?? throw new InvalidOperationException("La URL de la imagen por defecto no puede ser nula.");
            // Configurar la zona horaria local
            _timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneInfo.Local.Id);
        }

        /// <summary>
        /// Configura todas las mapeos para las órdenes de compra.
        /// </summary>
        public void ConfigureAllMappings()
        {
            ConfigureOrderItemsMappings();
            ConfigureOrderMappings();
        }

        /// <summary>
        /// Configura el mapeo de las órdenes de compra.
        /// </summary>
        private void ConfigureOrderMappings()
        {
            TypeAdapterConfig<Cart, Order>.NewConfig()
                .Map(dest => dest.OrderItems, src => src.CartItems.Select(i => i.Adapt<OrderItem>()).ToList())
                .Ignore(dest => dest.Id);

            TypeAdapterConfig<Order, OrderDetailDTO>.NewConfig()
                .Map(dest => dest.TransactionDate, src => TimeZoneInfo.ConvertTimeFromUtc(src.TransactionDate, _timeZoneInfo))
                .Map(dest => dest.TotalPrice, src => src.TotalPrice.ToString("C"))
                .Map(dest => dest.Items, src => src.OrderItems.Select(i => i.Adapt<OrderItemDTO>()).ToList());
        }

        /// <summary>
        /// Configura el mapeo de los items de las órdenes de compra. Se adptan de CartItem a OrderItem.
        /// </summary>
        private void ConfigureOrderItemsMappings()
        {
            TypeAdapterConfig<CartItem, OrderItem>.NewConfig()
                .Map(dest => dest.NameAtMoment, src => src.Product.Name)
                .Map(dest => dest.DescriptionAtMoment, src => src.Product.Description)
                .Map(dest => dest.UnitPriceAtMoment, src => src.Product.Price)
                .Map(dest => dest.ImageUrlAtMoment, src => src.Product.Images != null && src.Product.Images.Any() ? src.Product.Images.First().ImageUrl : _defaultImageUrl)
                .Map(dest => dest.SubtotalPrice, src => src.Quantity * src.Product.Price)
                .Ignore(dest => dest.Id);

            TypeAdapterConfig<OrderItem, OrderItemDTO>.NewConfig()
                .Map(dest => dest.ProductName, src => src.NameAtMoment)
                .Map(dest => dest.ProductDescription, src => src.DescriptionAtMoment)
                .Map(dest => dest.MainImageURL, src => src.ImageUrlAtMoment)
                .Map(dest => dest.UnitPriceAtMoment, src => src.UnitPriceAtMoment.ToString("C"))
                .Map(dest => dest.SubtotalPrice, src => src.SubtotalPrice.ToString("C"));
        }
    }
}
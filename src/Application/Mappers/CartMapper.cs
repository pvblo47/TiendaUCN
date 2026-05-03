using Mapster;
using TiendaUCN.src.Application.DTOs.CartDTO;
using TiendaUCN.src.Domain.Models;

namespace TiendaUCN.src.Application.Mappers
{
    public class CartMapper
    {
        private readonly IConfiguration _configuration;
        private readonly string? _defaultImageURL;
        public CartMapper(IConfiguration configuration)
        {
            _configuration = configuration;
            _defaultImageURL = _configuration.GetValue<string>("Products:DefaultImageUrl") ?? throw new InvalidOperationException("La URL de la imagen por defecto no puede ser nula.");
        }

        public void ConfigureAllMappings()
        {
            ConfigureCartMappings();
            ConfigureCartItemMapppings();
        }

        private void ConfigureCartMappings()
        {
            TypeAdapterConfig<Cart, CartDTO>.NewConfig()
                .Map(dest => dest.TotalPrice, src => src.TotalPrice.ToString("C"))
                .Map(dest => dest.Items, src => src.CartItems.Select(i => i.Adapt<CartItemDTO>()).ToList());
        }

        private void ConfigureCartItemMapppings()
        {
            TypeAdapterConfig<CartItem, CartItemDTO>.NewConfig()
                .Map(dest => dest.ProductName, src => src.Product.Name)
                .Map(dest => dest.ProductImageUrl, src => src.Product.Images.FirstOrDefault() != null ? src.Product.Images.First().ImageUrl : _defaultImageURL)
                .Map(dest => dest.ProductPrice, src => src.Product.Price.ToString("C"))
                .Map(dest => dest.TotalPrice, src => (src.Quantity * src.Product.Price).ToString("C"));
        }
    }
}
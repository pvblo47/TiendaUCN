using Mapster;
using Serilog;
using TiendaUCN.src.Application.DTOs.OrderDTO;
using TiendaUCN.src.Application.DTOs.ProductDTO;
using TiendaUCN.src.Application.Services.Interfaces;
using TiendaUCN.src.Domain.Models;
using TiendaUCN.src.Infrastructure.Repositories.Interfaces;

namespace TiendaUCN.src.Application.Services.Implements
{
    /// <summary>
    /// Servicio de órdenes.
    /// </summary>
    public class OrderService : IOrderService
    {
        /// <summary>
        /// Interfaz del repositorio de órdenes.
        /// </summary>
        private readonly IOrderRepository _orderRepository;

        /// <summary>
        /// Interfaz del repositorio de carritos.
        /// </summary>
        private readonly ICartRepository _cartRepository;

        /// <summary>
        /// Interfaz del repositorio de productos.
        /// </summary>
        private readonly IProductRepository _productRepository;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="OrderService"/>.
        /// </summary>
        /// <param name="orderRepository">Interfaz del repositorio de órdenes.</param>
        /// <param name="cartRepository">Interfaz del repositorio de carritos.</param>
        /// <param name="productRepository">Interfaz del repositorio de productos.</param>
        public OrderService(IOrderRepository orderRepository, ICartRepository cartRepository, IProductRepository productRepository)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
        }

        /// <summary>
        /// Crea una nueva orden.
        /// </summary>
        /// <param name="userId">Identificador del usuario.</param>
        /// <returns>Código de la orden creada.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<string> CreateOrderAsync(int userId)
        {
            // Obtener el carrito del usuario
            var cart = await _cartRepository.GetByUserIdAsync(userId)
                ?? throw new InvalidOperationException("No se encontró un carrito para el usuario.");

            // Validar que el carrito no esté vacío
            if (cart.CartItems.Count == 0)
            {
                Log.Information("El usuario {UserId} intentó crear una orden con un carrito vacío.", userId);
                throw new InvalidOperationException("No se puede crear una orden con un carrito vacío.");
            }

            // Generar un código único
            string code = await GenerateOrderCodeAsync();

            // Mapear el carrito a una orden
            Order order = cart.Adapt<Order>();
            order.Code = code;
            order.UserId = userId;

            // Guardar la orden en la base de datos
            var isCreated = await _orderRepository.CreateAsync(order);
            if (!isCreated)
            {
                Log.Error("Error al crear la orden para el usuario {UserId}.", userId);
                throw new Exception("No se pudo crear la orden. Por favor, intente nuevamente.");
            }

            // Actualizar el stock de los productos
            foreach (var item in cart.CartItems)
            {
                var newStock = item.Product.Stock - item.Quantity;
                await _productRepository.UpdateStockAsync(item.ProductId, newStock);
            }

            // Eliminar todos los items del carrito
            var isCleared = await _cartRepository.ClearCartItemsAsync(cart.Id);
            if (!isCleared)
            {
                Log.Error("Error al limpiar el carrito para el userId: {UserId}", userId);
                throw new Exception($"Error al limpiar el carrito para el userId: {userId}");
            }
            Log.Information("Carrito limpiado. CartId: {CartId}", cart.Id);

            // Actualizar el precio total del carrito a 0
            var newTotalPrice = 0;
            await _cartRepository.UpdateTotalPriceAsync(cart.Id, newTotalPrice);

            // Retornar el código de la orden creada
            return code;
        }

        /// <summary>
        /// Obtiene el detalle de una orden.
        /// </summary>
        /// <param name="orderCode">Código de la orden.</param>
        /// <param name="userId">Identificador del usuario.</param>
        /// <returns>Detalle de la orden.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<OrderDetailDTO> GetOrderDetailAsync(string orderCode, int userId)
        {
            // Obtener la orden por su código
            var order = await _orderRepository.GetByCodeAsync(orderCode, userId)
                ?? throw new InvalidOperationException("No se encontró una orden con el código proporcionado para el usuario.");

            // Mapear la orden a un DTO y retornarlo
            return order.Adapt<OrderDetailDTO>();
        }

        /// <summary>
        /// Obtiene las órdenes de un usuario con paginación y filtros.
        /// </summary>
        /// <param name="searchParams">Parámetros de búsqueda y paginación.</param>
        /// <param name="userId">Identificador del usuario.</param>
        /// <returns>Listado de órdenes.</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public async Task<ListedOrderDetailDTO> GetOrdersByUserIdAsync(SearchParamsDTO searchParams, int userId)
        {
            // Obtener las orders filtrados y el total de orders que cumplen con el filtro
            var (orders, totalCount) = await _orderRepository.GetFilteredForUserIdAsync(searchParams, userId);

            if (totalCount == 0)
            {
                Log.Information("No se encontraron productos que cumplan con los criterios de búsqueda para el customer. Filtros: {@SearchParams}", searchParams);
                throw new KeyNotFoundException("No se encontraron productos que cumplan con los criterios de búsqueda.");
            }

            var totalPages = (int)Math.Ceiling((double)totalCount / searchParams.PageSize);
            var ordersInPage = orders.Count();

            // Validar que la página solicitada no exceda el total de páginas disponibles
            if (searchParams.PageNumber > totalPages)
            {
                Log.Information("No se encontraron productos en la página solicitada para el customer. Filtros: {@SearchParams}", searchParams);
                throw new ArgumentOutOfRangeException($"La página {searchParams.PageNumber} no existe. Total: {totalPages}.");
            }

            // Mapear las órdenes a un DTO de listado
            var listedOrders = new ListedOrderDetailDTO
            {
                Orders = orders.Adapt<List<OrderDetailDTO>>(),
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = searchParams.PageNumber,
                PageSize = searchParams.PageSize,
                OrdersInPage = ordersInPage
            };

            // Retornar el DTO
            return listedOrders;
        }

        /// <summary>
        /// Genera un código único para la orden.
        /// </summary>
        /// <returns>Código único para la orden.</returns>
        private async Task<string> GenerateOrderCodeAsync()
        {
            string code;
            do
            {
                // Generar un código único para la orden
                var timestamp = DateTime.UtcNow.ToString("yyMMddHHmmss");
                var random = Random.Shared.Next(100, 999);
                code = $"ORD-{timestamp}-{random}";
            }
            // Verificar que el código generado no exista en la base de datos
            while (await _orderRepository.ExistsByCodeAsync(code));

            return code;
        }
    }
}
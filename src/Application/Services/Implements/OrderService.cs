using Mapster;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TiendaUCN.src.Application.DTOs.OrderDTO;
using TiendaUCN.src.Application.DTOs.ProductDTO;
using TiendaUCN.src.Application.Services.Interfaces;
using TiendaUCN.src.Domain.Models;
using TiendaUCN.src.Infrastructure.Data;
using TiendaUCN.src.Infrastructure.Repositories.Interfaces;

namespace TiendaUCN.src.Application.Services.Implements
{
    /// <summary>
    /// Servicio de ordenes.
    /// </summary>
    public class OrderService : IOrderService
    {
        /// <summary>
        /// Interfaz del repositorio de ordenes.
        /// </summary>
        private readonly IOrderRepository _orderRepository;

        /// <summary>
        /// Interfaz del repositorio de carritos.
        /// </summary>
        private readonly ICartRepository _cartRepository;

        /// <summary>
        /// Contexto de datos para el flujo transaccional de compra.
        /// </summary>
        private readonly DataContext _context;

        /// <summary>
        /// Interfaz del repositorio de productos.
        /// </summary>
        private readonly IProductRepository _productRepository;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="OrderService"/>.
        /// </summary>
        /// <param name="orderRepository">Interfaz del repositorio de ordenes.</param>
        /// <param name="cartRepository">Interfaz del repositorio de carritos.</param>
        /// <param name="productRepository">Interfaz del repositorio de productos.</param>
        /// <param name="context">Contexto de datos.</param>
        public OrderService(IOrderRepository orderRepository, ICartRepository cartRepository, IProductRepository productRepository, DataContext context)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _context = context;
        }

        /// <summary>
        /// Crea una nueva orden.
        /// </summary>
        /// <param name="userId">Identificador del usuario.</param>
        /// <returns>Codigo de la orden creada.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<string> CreateOrderAsync(int userId)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Obtener el carrito del usuario
                var cart = await _cartRepository.GetByUserIdAsync(userId)
                    ?? throw new InvalidOperationException("No se encontro un carrito para el usuario.");

                // Validar que el carrito no este vacio
                if (cart.CartItems.Count == 0)
                {
                    Log.Information("El usuario {UserId} intento crear una orden con un carrito vacio.", userId);
                    throw new InvalidOperationException("No se puede crear una orden con un carrito vacio.");
                }

                var validatedProducts = new Dictionary<int, Product>();

                // Revalidar el estado real de los productos directamente en la base de datos
                foreach (var item in cart.CartItems)
                {
                    var currentProduct = await _context.Products
                        .Include(p => p.Images)
                        .FirstOrDefaultAsync(p =>
                            p.Id == item.ProductId &&
                            p.IsActive &&
                            !p.IsDeleted);

                    if (currentProduct == null)
                    {
                        Log.Information("Producto no disponible al confirmar compra. ProductId: {ProductId}, UserId: {UserId}", item.ProductId, userId);
                        throw new InvalidOperationException($"El producto con ID {item.ProductId} no esta disponible para la compra.");
                    }

                    if (currentProduct.Stock < item.Quantity)
                    {
                        Log.Information("Stock insuficiente al confirmar compra. ProductId: {ProductId}, Stock: {Stock}, Requested: {Requested}, UserId: {UserId}", item.ProductId, currentProduct.Stock, item.Quantity, userId);
                        throw new InvalidOperationException($"Stock insuficiente para el producto '{currentProduct.Name}'. Stock disponible: {currentProduct.Stock}.");
                    }

                    validatedProducts[item.ProductId] = currentProduct;
                    item.Product = currentProduct;
                }

                // Generar un codigo unico
                string code = await GenerateOrderCodeAsync();

                // Mapear el carrito a una orden reutilizando el mapeo existente y los datos revalidados
                Order order = cart.Adapt<Order>();
                order.Code = code;
                order.UserId = userId;
                order.TotalPrice = cart.CartItems.Sum(item => validatedProducts[item.ProductId].Price * item.Quantity);

                // Guardar la orden en la base de datos
                var isCreated = await _orderRepository.CreateAsync(order);
                if (!isCreated)
                {
                    Log.Error("Error al crear la orden para el usuario {UserId}.", userId);
                    throw new InvalidOperationException("No se pudo crear la orden. Por favor, intente nuevamente.");
                }

                // Actualizar el stock real de los productos validados
                foreach (var item in cart.CartItems)
                {
                    var currentProduct = validatedProducts[item.ProductId];
                    var newStock = currentProduct.Stock - item.Quantity;
                    var isUpdated = await _productRepository.UpdateStockAsync(item.ProductId, newStock);

                    if (!isUpdated)
                    {
                        Log.Error("No se pudo actualizar el stock del producto {ProductId} para la orden {OrderCode}.", item.ProductId, code);
                        throw new InvalidOperationException($"No se pudo actualizar el stock del producto '{currentProduct.Name}'.");
                    }
                }

                // Eliminar todos los items del carrito
                var isCleared = await _cartRepository.ClearCartItemsAsync(cart.Id);
                if (!isCleared)
                {
                    Log.Error("Error al limpiar el carrito para el userId: {UserId}", userId);
                    throw new InvalidOperationException("No se pudo vaciar el carrito despues de crear la orden.");
                }
                Log.Information("Carrito limpiado. CartId: {CartId}", cart.Id);

                // Actualizar el precio total del carrito a 0
                await _cartRepository.UpdateTotalPriceAsync(cart.Id, 0);

                await transaction.CommitAsync();
                return code;
            }
            catch (InvalidOperationException)
            {
                await transaction.RollbackAsync();
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Log.Error(ex, "Error transaccional al crear la orden para el usuario {UserId}.", userId);
                throw new InvalidOperationException("No se pudo confirmar la compra. Intente nuevamente.", ex);
            }
        }

        /// <summary>
        /// Obtiene el detalle de una orden.
        /// </summary>
        /// <param name="orderCode">Codigo de la orden.</param>
        /// <param name="userId">Identificador del usuario.</param>
        /// <returns>Detalle de la orden.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<OrderDetailDTO> GetOrderDetailAsync(string orderCode, int userId)
        {
            // Obtener la orden por su codigo
            var order = await _orderRepository.GetByCodeAsync(orderCode, userId)
                ?? throw new InvalidOperationException("No se encontro una orden con el codigo proporcionado para el usuario.");

            // Mapear la orden a un DTO y retornarlo
            return order.Adapt<OrderDetailDTO>();
        }

        /// <summary>
        /// Obtiene las ordenes de un usuario con paginacion y filtros.
        /// </summary>
        /// <param name="searchParams">Parametros de busqueda y paginacion.</param>
        /// <param name="userId">Identificador del usuario.</param>
        /// <returns>Listado de ordenes.</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public async Task<ListedOrderDetailDTO> GetOrdersByUserIdAsync(SearchParamsDTO searchParams, int userId)
        {
            // Obtener las orders filtrados y el total de orders que cumplen con el filtro
            var (orders, totalCount) = await _orderRepository.GetFilteredForUserIdAsync(searchParams, userId);

            if (totalCount == 0)
            {
                Log.Information("No se encontraron productos que cumplan con los criterios de busqueda para el customer. Filtros: {@SearchParams}", searchParams);
                throw new KeyNotFoundException("No se encontraron productos que cumplan con los criterios de busqueda.");
            }

            var totalPages = (int)Math.Ceiling((double)totalCount / searchParams.PageSize);
            var ordersInPage = orders.Count();

            // Validar que la pagina solicitada no exceda el total de paginas disponibles
            if (searchParams.PageNumber > totalPages)
            {
                Log.Information("No se encontraron productos en la pagina solicitada para el customer. Filtros: {@SearchParams}", searchParams);
                throw new ArgumentOutOfRangeException($"La pagina {searchParams.PageNumber} no existe. Total: {totalPages}.");
            }

            // Mapear las ordenes a un DTO de listado
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
        /// Genera un codigo unico para la orden.
        /// </summary>
        /// <returns>Codigo unico para la orden.</returns>
        private async Task<string> GenerateOrderCodeAsync()
        {
            string code;
            do
            {
                // Generar un codigo unico para la orden
                var timestamp = DateTime.UtcNow.ToString("yyMMddHHmmss");
                var random = Random.Shared.Next(100, 999);
                code = $"ORD-{timestamp}-{random}";
            }
            // Verificar que el codigo generado no exista en la base de datos
            while (await _orderRepository.ExistsByCodeAsync(code));

            return code;
        }
    }
}

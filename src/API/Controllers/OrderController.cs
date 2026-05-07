using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TiendaUCN.src.Application.DTOs.BaseResponse;
using TiendaUCN.src.Application.DTOs.OrderDTO;
using TiendaUCN.src.Application.DTOs.ProductDTO;
using TiendaUCN.src.Application.Services.Interfaces;

namespace TiendaUCN.src.API.Controllers
{
    /// <summary>
    /// Controlador de órdenes.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        /// <summary>
        /// Interfaz del servicio de órdenes.
        /// </summary>
        private readonly IOrderService _orderService;

        /// <summary>
        /// Constructor del controlador de órdenes.
        /// </summary>
        /// <param name="orderService">Interfaz del servicio de órdenes</param>
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Endpoint para crear una nueva orden.
        /// </summary>
        /// <returns>Codigo de la orden creada</returns>
        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CreateOrder()
        {
            var userId = GetUserId();
            var result = await _orderService.CreateOrderAsync(userId!.Value);
            return Created($"api/order/{result}", new GenericResponse<string>("Orden creada exitosamente", result));
        }

        /// <summary>
        /// Endpoint para obtener los detalles de una orden específica.
        /// </summary>
        /// <param name="orderCode">Código de la orden</param>
        /// <returns>Detalles de la orden</returns>
        [HttpGet("{orderCode}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetOrderDetail([FromRoute] string orderCode)
        {
            var userId = GetUserId();
            var result = await _orderService.GetOrderDetailAsync(orderCode, userId!.Value);
            return Ok(new GenericResponse<OrderDetailDTO>("Detalles de la orden", result));
        }

        /// <summary>
        /// Endpoint para obtener las órdenes del usuario actual.
        /// </summary>
        /// <param name="searchParams">Parámetros de búsqueda</param>
        /// <returns>Lista de órdenes</returns>
        [HttpGet("user-orders")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetUserOrders([FromQuery] SearchParamsDTO searchParams)
        {
            var userId = GetUserId();
            var result = _orderService.GetOrdersByUserIdAsync(searchParams, userId!.Value);
            return Ok(new GenericResponse<ListedOrderDetailDTO>("Órdenes del usuario obtenidas exitosamente", await result));
        }

        /// <summary>
        /// Método auxiliar para obtener el userId
        /// </summary>
        /// <returns>El ID del usuario o null si no se encuentra</returns>
        private int? GetUserId()
        {
            // Intentar obtener el userId del token JWT
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Si no se encuentra el claim o no se puede convertir a int, retornar null
            if (int.TryParse(userIdClaim, out int userId))
                return userId;

            return null;
        }
    }
}
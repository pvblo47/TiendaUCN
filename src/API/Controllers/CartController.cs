using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TiendaUCN.src.Application.DTOs.BaseResponse;
using TiendaUCN.src.Application.DTOs.CartDTO;
using TiendaUCN.src.Application.Services.Interfaces;

namespace TiendaUCN.src.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetCart()
        {
            var buyerId = GetBuyerId();
            var userId = GetUserId();
            var cart = await _cartService.CreateOrGetCartAsync(buyerId, userId);
            return Ok(new GenericResponse<CartDTO>("Carrito obtenido exitosamente", cart));
        }

        [HttpPost("items")]
        [AllowAnonymous]
        public async Task<IActionResult> AddCartItem([FromBody] AddChangeCartItemDTO addCartItemDTO)
        {
            var buyerId = GetBuyerId();
            var userId = GetUserId();
            var cart = await _cartService.AddCartItemAsync(buyerId, addCartItemDTO, userId);
            return Ok(new GenericResponse<CartDTO>("Item agregado al carrito exitosamente", cart));
        }

        [HttpPatch("items")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateItemQuantity([FromBody] AddChangeCartItemDTO changeCartItemDTO)
        {
            var buyerId = GetBuyerId();
            var userId = GetUserId();
            var cart = await _cartService.UpdateCartItemQuantityAsync(buyerId, changeCartItemDTO, userId);
            return Ok(new GenericResponse<CartDTO>("Cantidad del item actualizada exitosamente", cart));
        }

        [HttpDelete("items/{productId}")]
        [AllowAnonymous]
        public async Task<IActionResult> RemoveItem([FromRoute] int productId)
        {
            var buyerId = GetBuyerId();
            var userId = GetUserId();
            var cart = await _cartService.RemoveCartItemAsync(buyerId, productId, userId);
            return Ok(new GenericResponse<CartDTO>("Item removido del carrito exitosamente", cart));
        }

        [HttpPut("clear")]
        [AllowAnonymous]
        public async Task<IActionResult> ClearCart()
        {
            var buyerId = GetBuyerId();
            var userId = GetUserId();
            var cart = await _cartService.ClearCartAsync(buyerId, userId);
            return Ok(new GenericResponse<CartDTO>("Carrito limpiado exitosamente", cart));
        }

        /// <summary>
        /// Endpoint para realizar el checkout del carrito de compras.
        /// </summary>
        /// <returns>DTO del carrito de compras</returns>
        [HttpPost("checkout")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CheckoutCart()
        {
            var userId = GetUserId();
            var result = await _cartService.CheckoutCartAsync(userId!.Value);
            return Ok(new GenericResponse<CheckoutResultDTO>("Checkout realizado exitosamente", result));
        }

        private string GetBuyerId()
        {
            // Intentar obtener el buyerId del contexto HTTP
            var buyerId = HttpContext.Items["BuyerId"]?.ToString();

            if (string.IsNullOrEmpty(buyerId))
            {
                throw new Exception("No se encontró el id del comprador.");
            }
            return buyerId;
        }

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
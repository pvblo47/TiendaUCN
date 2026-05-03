using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TiendaUCN.src.Application.DTOs.BaseResponse;
using TiendaUCN.src.Application.DTOs.ProductDTO;
using TiendaUCN.src.Application.DTOs.ProductDTO.Admin;
using TiendaUCN.src.Application.DTOs.ProductDTO.Customer;
using TiendaUCN.src.Application.Services.Interfaces;

namespace TiendaUCN.src.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromForm] CreateProductDTO createProductDTO)
        {
            var result = await _productService.CreateProductAsync(createProductDTO);
            return Created($"api/product/{result}", new GenericResponse<string>("Producto creado exitosamente", result));
        }

        [HttpPatch("switch-status/{id}")]
        public async Task<IActionResult> SwitchStatusProduct([FromRoute] int id)
        {
            var result = await _productService.SwitchStatusProductAsync(id);
            return Ok(new GenericResponse<string>("Estado del producto cambiado exitosamente", result));
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductByIdForCustomer([FromRoute] int id)
        {
            var result = await _productService.GetProductByIdForCustomerAsync(id);
            return Ok(new GenericResponse<ProductDetailCustomerDTO>("Producto encontrado exitosamente", result));
        }

        [HttpGet("admin/{id}")]
        public async Task<IActionResult> GetProductByIdForAdmin([FromRoute] int id)
        {
            var result = await _productService.GetProductByIdForAdminAsync(id);
            return Ok(new GenericResponse<ProductDetailAdminDTO>("Producto encontrado exitosamente", result));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct([FromRoute] int id)
        {
            await _productService.DeleteProductAsync(id);
            return Ok(new GenericResponse<string>("Producto eliminado exitosamente", null));
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ListProductsForCustomer([FromQuery] SearchParamsDTO searchParams)
        {
            var result = await _productService.GetListedProductsForCustomerAsync(searchParams);
            return Ok(new GenericResponse<ListedProductsForCustomerDTO>("Productos encontrados exitosamente", result));
        }

        [HttpGet("admin")]
        public async Task<IActionResult> ListProductsForAdmin([FromQuery] SearchParamsDTO searchParams)
        {
            var result = await _productService.GetListedProductsForAdminAsync(searchParams);
            return Ok(new GenericResponse<ListedProductsForAdminDTO>("Productos encontrados exitosamente", result));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct([FromRoute] int id, [FromForm] UpdateProductDTO updateProductDTO)
        {
            await _productService.UpdateProductAsync(id, updateProductDTO);
            return Ok(new GenericResponse<string>("Producto actualizado exitosamente", null));
        }
    }
}
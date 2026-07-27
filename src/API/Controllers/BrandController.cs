using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TiendaUCN.src.Application.DTOs.BaseResponse;
using TiendaUCN.src.Application.DTOs.BrandCategoryDTO;
using TiendaUCN.src.Application.Services.Interfaces;

namespace TiendaUCN.src.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class BrandController : ControllerBase
    {
        private readonly IBrandService _brandService;
        public BrandController(IBrandService brandService)
        {
            _brandService = brandService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateBrand([FromBody] CreateBrandCategoryDTO createBrandDTO)
        {
            var message = await _brandService.CreateBrandAsync(createBrandDTO);
            return Ok(new GenericResponse<string>(message, null));
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> UpdateBrand([FromRoute] int id, [FromBody] UpdateBrandCategoryDTO updateBrandDTO)
        {
            var message = await _brandService.UpdateBrandAsync(id, updateBrandDTO);
            return Ok(new GenericResponse<string>(message, null));
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteBrand([FromRoute] int id)
        {
            var message = await _brandService.DeleteBrandAsync(id);
            return Ok(new GenericResponse<string>(message, null));
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllActiveBrands()
        {
            var result = await _brandService.GetAllActiveBrandsAsync();
            return Ok(new GenericResponse<List<CatalogItemDTO>>("Marcas encontradas exitosamente", result));
        }
    }
}
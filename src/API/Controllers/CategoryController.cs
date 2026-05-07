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
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CreateBrandCategoryDTO createCategoryDTO)
        {
            var message = await _categoryService.CreateCategoryAsync(createCategoryDTO);
            return Ok(new GenericResponse<string>(message, null));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory([FromRoute] int id, [FromBody] UpdateBrandCategoryDTO updateCategoryDTO)
        {
            var message = await _categoryService.UpdateCategoryAsync(id, updateCategoryDTO);
            return Ok(new GenericResponse<string>(message, null));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory([FromRoute] int id)
        {
            var message = await _categoryService.DeleteCategoryAsync(id);
            return Ok(new GenericResponse<string>(message, null));
        }
    }
}
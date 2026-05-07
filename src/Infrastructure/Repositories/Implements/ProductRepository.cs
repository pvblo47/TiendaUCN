using Microsoft.EntityFrameworkCore;
using TiendaUCN.src.Application.DTOs.ProductDTO;
using TiendaUCN.src.Domain.Models;
using TiendaUCN.src.Infrastructure.Data;
using TiendaUCN.src.Infrastructure.Repositories.Interfaces;

namespace TiendaUCN.src.Infrastructure.Repositories.Implements
{
    public class ProductRepository : IProductRepository
    {
        private readonly DataContext _context;
        public ProductRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistsByNameAndBrandAsync(string name, string brandName)
        {
            // Verificar producto unico, antes de crear un producto. 
            // El producto no debe existir, por la combinacion de su nombre y marca, y ambos modelos no deben estar eliminados
            return await _context.Products
                .Include(p => p.Brand)
                .AnyAsync(p =>
                    p.Name.ToLower() == name.ToLower() &&
                    p.Brand.Name.ToLower() == brandName.ToLower() &&
                    p.IsDeleted == false &&
                    p.Brand.IsDeleted == false);
        }

        public async Task<bool> CreateAsync(Product product)
        {
            _context.Products.Add(product);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ExistsByIdAsync(int id)
        {
            // Verificar producto existente.
            // Para un admin.
            // El producto debe existir y no estar eliminado
            return await _context.Products
                .AnyAsync(p => p.Id == id && p.IsDeleted == false);
        }

        public async Task<bool> SwitchStatusAsync(int id)
        {
            return await _context.Products
                .Where(p => p.Id == id && p.IsDeleted == false)
                .ExecuteUpdateAsync(p =>
                    p.SetProperty(p => p.IsActive, p => !p.IsActive)) > 0;
        }

        public async Task<string?> GetStatusAsync(int id)
        {
            return await _context.Products
                .Where(p => p.Id == id && p.IsDeleted == false)
                .Select(p => p.IsActive ? "Activo" : "Inactivo")
                .FirstOrDefaultAsync();
        }

        public async Task<bool> ExistsByIdCustomerAsync(int id)
        {
            // Verificar producto existente.
            // Para un cliente.
            // El producto debe existir, esté activo y no estar eliminado
            return await _context.Products
                .AnyAsync(p =>
                    p.Id == id &&
                    p.IsActive == true &&
                    p.IsDeleted == false);
        }

        public async Task<Product?> GetProductByIdForCustomerAsync(int id)
        {
            // Obtener producto por id.
            // Para un cliente.
            // El producto debe existir, estar activo y no estar eliminado
            return await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p =>
                    p.Id == id &&
                    p.IsDeleted == false &&
                    p.IsActive == true);
        }

        public async Task<Product?> GetProductByIdForAdminAsync(int id)
        {
            // Obtener producto por id.
            // Para un admin.
            // El producto debe existir y no estar eliminado
            return await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p =>
                    p.Id == id &&
                    p.IsDeleted == false);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            // Eliminar producto por id.
            // El producto debe existir y no estar eliminado
            return await _context.Products
                .Where(p => p.Id == id && p.IsDeleted == false)
                .ExecuteUpdateAsync(p =>
                    p.SetProperty(p => p.IsDeleted, true)) > 0;
        }

        public async Task<(IEnumerable<Product> products, int totalCount)> GetFilteredForAdminAsync(SearchParamsDTO searchParams)
        {
            // Obtener queryable de productos que no estén eliminados
            var query = _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.Images.Take(1))
                .Where(p => p.IsDeleted == false)
                .AsNoTracking(); // Para mejorar el rendimiento en consultas de solo lectura

            // Aplicar filtro de búsqueda si se proporciona un término de búsqueda
            if (!string.IsNullOrWhiteSpace(searchParams.SearchTerm))
            {
                var searchTerm = searchParams.SearchTerm.Trim().ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(searchTerm) ||
                    p.Description.ToLower().Contains(searchTerm) ||
                    p.Price.ToString().Contains(searchTerm) ||
                    p.Stock.ToString().Contains(searchTerm) ||
                    p.Category.Name.ToLower().Contains(searchTerm) ||
                    (p.Category.Description != null && p.Category.Description.ToLower().Contains(searchTerm)) ||
                    p.Brand.Name.ToLower().Contains(searchTerm) ||
                    (p.Brand.Description != null && p.Brand.Description.ToLower().Contains(searchTerm)));
            }

            // Obtener el total de productos que cumplen con el filtro
            int totalCount = await query.CountAsync();

            // Aplicar paginación
            var products = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((searchParams.PageNumber - 1) * searchParams.PageSize)
                .Take(searchParams.PageSize)
                .ToArrayAsync();

            // Ej: Si el tamaño de página es 10, y el número de página es 2
            // Se omitirán los primeros 10 productos y se tomarán los siguientes 10 productos para mostrar en la página 2.

            return (products, totalCount);
        }

        public async Task<(IEnumerable<Product> products, int totalCount)> GetFilteredForCustomerAsync(SearchParamsDTO searchParams)
        {
            // Obtener queryable de productos que estén activos y no eliminados
            var query = _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.Images.Take(1))
                .Where(p => p.IsDeleted == false && p.IsActive == true)
                .AsNoTracking(); // Para mejorar el rendimiento en consultas de solo lectura

            // Aplicar filtro de búsqueda si se proporciona un término de búsqueda
            if (!string.IsNullOrWhiteSpace(searchParams.SearchTerm))
            {
                var searchTerm = searchParams.SearchTerm.Trim().ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(searchTerm) ||
                    p.Description.ToLower().Contains(searchTerm) ||
                    p.Price.ToString().Contains(searchTerm) ||
                    p.Stock.ToString().Contains(searchTerm) ||
                    p.Category.Name.ToLower().Contains(searchTerm) ||
                    (p.Category.Description != null && p.Category.Description.ToLower().Contains(searchTerm)) ||
                    p.Brand.Name.ToLower().Contains(searchTerm) ||
                    (p.Brand.Description != null && p.Brand.Description.ToLower().Contains(searchTerm)));
            }

            // Obtener el total de productos que cumplen con el filtro
            int totalCount = await query.CountAsync();

            // Aplicar paginación
            var products = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((searchParams.PageNumber - 1) * searchParams.PageSize)
                .Take(searchParams.PageSize)
                .ToArrayAsync();

            return (products, totalCount);
        }

        public async Task<bool> UpdateAsync(Product product)
        {
            return await _context.Products
                .Where(p => p.Id == product.Id && p.IsDeleted == false)
                .ExecuteUpdateAsync(p => p
                    .SetProperty(p => p.Name, product.Name)
                    .SetProperty(p => p.Description, product.Description)
                    .SetProperty(p => p.Price, product.Price)
                    .SetProperty(p => p.Stock, product.Stock)
                    .SetProperty(p => p.CategoryId, product.CategoryId)
                    .SetProperty(p => p.BrandId, product.BrandId)) > 0;
        }

        /// <summary>
        /// Actualiza el stock de un producto por su ID.
        /// </summary>
        /// <param name="productId">El ID del producto</param>
        /// <param name="newStock">El nuevo stock</param>
        /// <returns>true si lo actualiza, false si no</returns>
        public async Task<bool> UpdateStockAsync(int productId, int newStock)
        {
            return await _context.Products
                .Where(p => p.Id == productId && p.IsDeleted == false)
                .ExecuteUpdateAsync(p =>
                    p.SetProperty(p => p.Stock, newStock)) > 0;
        }

    }
}
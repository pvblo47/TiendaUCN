using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Diagnostics;
using TiendaUCN.src.Application.DTOs.ProductDTO;
using TiendaUCN.src.Application.DTOs.ProductDTO.Admin;
using TiendaUCN.src.Application.DTOs.ProductDTO.Customer;
using TiendaUCN.src.Application.Services.Interfaces;
using TiendaUCN.src.Domain.Models;
using TiendaUCN.src.Infrastructure.Repositories.Interfaces;

namespace TiendaUCN.src.Application.Services.Implements
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;

        private IImageService ImageService => _serviceProvider.GetRequiredService<IImageService>();

        public ProductService(IProductRepository productRepository, IBrandRepository brandRepository, ICategoryRepository categoryRepository, IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _productRepository = productRepository;
            _brandRepository = brandRepository;
            _categoryRepository = categoryRepository;
            _configuration = configuration;
            _serviceProvider = serviceProvider;
        }

        public async Task<string> CreateProductAsync(CreateProductDTO createProductDTO)
        {
            // Verificar si la categoría existe
            var categoryExists = await _categoryRepository.ExistsByNameAsync(createProductDTO.CategoryName);
            if (!categoryExists)
            {
                Log.Error("La categoría no existe: {CategoryName}", createProductDTO.CategoryName);
                throw new Exception("La categoría no existe: " + createProductDTO.CategoryName);
            }

            // Verificar si la marca existe
            var brandExists = await _brandRepository.ExistsByNameAsync(createProductDTO.BrandName);
            if (!brandExists)
            {
                Log.Error("La marca no existe: {BrandName}", createProductDTO.BrandName);
                throw new Exception("La marca no existe: " + createProductDTO.BrandName);
            }

            // Verificar si el producto ya existe, por la combinacion de su nombre y marca
            var productExists = await _productRepository.ExistsByNameAndBrandAsync(createProductDTO.Name, createProductDTO.BrandName);
            if (productExists)
            {
                Log.Error("Ya existe un producto con el mismo nombre y marca: {Name} - {Brand}", createProductDTO.Name, createProductDTO.BrandName);
                throw new Exception("Ya existe un producto con el mismo nombre y marca: " + createProductDTO.Name + " - " + createProductDTO.BrandName);
            }

            // Crear el producto
            var product = createProductDTO.Adapt<Product>();
            product.CategoryId = await _categoryRepository.GetIdByNameAsync(createProductDTO.CategoryName);
            product.BrandId = await _brandRepository.GetIdByNameAsync(createProductDTO.BrandName);

            // Guardar el producto en la base de datos
            var isCreated = await _productRepository.CreateAsync(product);
            if (!isCreated)
            {
                Log.Error("Error al crear el producto: {Name}", createProductDTO.Name);
                throw new Exception("Error al crear el producto: " + createProductDTO.Name);
            }

            // Subir las imágenes asociadas al producto
            foreach (var image in createProductDTO.ImagesFiles)
            {
                Log.Information("Imagen asociada al producto: {@Image}", image);
                await ImageService.UploadAsync(image, product.Id);
            }

            return product.Id.ToString();
        }

        public async Task<string> SwitchStatusProductAsync(int id)
        {
            var productExists = await _productRepository.ExistsByIdAsync(id);
            if (!productExists)
            {
                Log.Error("Producto no encontrado con ID: {ProductId}", id);
                throw new Exception("Producto no encontrado con ID: " + id);
            }

            var isUpdated = await _productRepository.SwitchStatusAsync(id);
            if (!isUpdated)
            {
                Log.Error("Error al cambiar el estado del producto con ID: {ProductId}", id);
                throw new Exception("Error al cambiar el estado del producto con ID: " + id);
            }

            var statusUpdated = await _productRepository.GetStatusAsync(id);
            return "Se cambio el estado a: " + statusUpdated;
        }

        public async Task<ProductDetailCustomerDTO> GetProductByIdForCustomerAsync(int id)
        {
            // Verificar si el producto existe
            var productExists = await _productRepository.ExistsByIdCustomerAsync(id);
            if (!productExists)
            {
                Log.Error("Producto no encontrado con ID: {ProductId}", id);
                throw new KeyNotFoundException("Producto no encontrado con ID: " + id);
            }

            // Obtener el producto de la base de datos
            var product = await _productRepository.GetProductByIdForCustomerAsync(id)
                ?? throw new KeyNotFoundException("Producto no encontrado con ID: " + id);

            // Si la categoría del producto está eliminada, asignar un nombre y descripción genéricos
            if (product.Category.IsDeleted)
            {
                product.Category.Name = "Categoría no disponible";
                product.Category.Description = "Categoría no disponible";
            }

            // Si la marca del producto está eliminada, asignar un nombre y descripción genéricos
            if (product.Brand.IsDeleted)
            {
                product.Brand.Name = "Marca no disponible";
                product.Brand.Description = "Marca no disponible";
            }

            // Mapear el producto a un DTO para el cliente
            var productDetailCustomerDTO = product.Adapt<ProductDetailCustomerDTO>();
            return productDetailCustomerDTO!;
        }

        public async Task<ProductDetailAdminDTO> GetProductByIdForAdminAsync(int id)
        {
            // Verificar si el producto existe
            var productExists = await _productRepository.ExistsByIdAsync(id);
            if (!productExists)
            {
                Log.Error("Producto no encontrado con ID: {ProductId}", id);
                throw new KeyNotFoundException("Producto no encontrado con ID: " + id);
            }

            // Obtener el producto de la base de datos
            Product product = await _productRepository.GetProductByIdForAdminAsync(id)
                ?? throw new KeyNotFoundException("Producto no encontrado con ID: " + id);

            // Si la categoría del producto está eliminada, asignar un nombre y descripción genéricos
            if (product.Category.IsDeleted)
            {
                product.Category.Name = "Categoría eliminada";
                product.Category.Description = "Categoría eliminada";
            }

            // Si la marca del producto está eliminada, asignar un nombre y descripción genéricos
            if (product.Brand.IsDeleted)
            {
                product.Brand.Name = "Marca eliminada";
                product.Brand.Description = "Marca eliminada";
            }

            // Mapear el producto a un DTO para el admin
            var productDetailAdminDTO = product.Adapt<ProductDetailAdminDTO>();
            return productDetailAdminDTO!;
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await _productRepository.GetProductByIdForAdminAsync(id);
            if (product == null)
            {
                Log.Error("Producto no encontrado con ID: {ProductId}", id);
                throw new KeyNotFoundException("Producto no encontrado con ID: " + id);
            }

            // Eliminar imágenes asociadas en Cloudinary
            if (product.Images != null && product.Images.Any())
            {
                foreach (var image in product.Images)
                {
                    try
                    {
                        await ImageService.DeleteAsync(image.PublicId);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error al eliminar la imagen en Cloudinary con PublicId: {PublicId}", image.PublicId);
                        // Dependiendo de la regla de negocio, se podría lanzar la excepción o solo registrar el error
                    }
                }
            }

            var isDeleted = await _productRepository.DeleteAsync(id);
            if (!isDeleted)
            {
                Log.Error("Error al eliminar el producto con ID: {ProductId}", id);
                throw new Exception("Error al eliminar el producto con ID: " + id);
            }
        }

        public async Task<ListedProductsForAdminDTO> GetListedProductsForAdminAsync(SearchParamsDTO searchParams)
        {
            // Obtener los productos filtrados y el total de productos que cumplen con el filtro
            var (products, totalCount) = await _productRepository.GetFilteredForAdminAsync(searchParams);

            if (totalCount == 0)
            {
                Log.Information("No se encontraron productos que cumplan con los criterios de búsqueda para el admin. Filtros: {@SearchParams}", searchParams);
                throw new KeyNotFoundException("No se encontraron productos que cumplan con los criterios de búsqueda para el admin.");
            }

            var totalPages = (int)Math.Ceiling((double)totalCount / searchParams.PageSize);
            var productsInPage = products.Count();

            if (searchParams.PageNumber > totalPages)
            {
                Log.Information("No se encontraron productos en la página solicitada para el admin. Filtros: {@SearchParams}", searchParams);
                throw new KeyNotFoundException($"La página {searchParams.PageNumber} no existe. Total: {totalPages}.");
            }

            // Convertimos los productos filtrados a DTOs para la respuesta
            return new ListedProductsForAdminDTO
            {
                Products = products.Adapt<List<ProductForAdminDTO>>(),
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = searchParams.PageNumber,
                PageSize = searchParams.PageSize,
                ProductsInPage = productsInPage
            };
        }

        public async Task<ListedProductsForCustomerDTO> GetListedProductsForCustomerAsync(SearchParamsDTO searchParams)
        {
            // Obtener los productos filtrados y el total de productos que cumplen con el filtro
            var (products, totalCount) = await _productRepository.GetFilteredForCustomerAsync(searchParams);

            if (totalCount == 0)
            {
                Log.Information("No se encontraron productos que cumplan con los criterios de búsqueda para el customer. Filtros: {@SearchParams}", searchParams);
                throw new KeyNotFoundException("No se encontraron productos que cumplan con los criterios de búsqueda.");
            }

            var totalPages = (int)Math.Ceiling((double)totalCount / searchParams.PageSize);
            var productsInPage = products.Count();

            if (searchParams.PageNumber > totalPages)
            {
                Log.Information("No se encontraron productos en la página solicitada para el customer. Filtros: {@SearchParams}", searchParams);
                throw new KeyNotFoundException($"La página {searchParams.PageNumber} no existe. Total: {totalPages}.");
            }

            // Convertimos los productos filtrados a DTOs para la respuesta
            return new ListedProductsForCustomerDTO
            {
                Products = products.Adapt<List<ProductForCustomerDTO>>(),
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = searchParams.PageNumber,
                PageSize = searchParams.PageSize,
                ProductsInPage = productsInPage
            };
        }

        public async Task UpdateProductAsync(int id, UpdateProductDTO updateProductDTO)
        {
            // Normalizar al inicio del método
            updateProductDTO.Name = string.IsNullOrWhiteSpace(updateProductDTO.Name) ? null : updateProductDTO.Name.ToLower().Trim();
            updateProductDTO.Description = string.IsNullOrWhiteSpace(updateProductDTO.Description) ? null : updateProductDTO.Description.ToLower().Trim();
            updateProductDTO.CategoryName = string.IsNullOrWhiteSpace(updateProductDTO.CategoryName) ? null : updateProductDTO.CategoryName.ToLower().Trim();
            updateProductDTO.BrandName = string.IsNullOrWhiteSpace(updateProductDTO.BrandName) ? null : updateProductDTO.BrandName.ToLower().Trim();

            // Filtrar strings vacíos de la lista
            updateProductDTO.ImagesToRemove = updateProductDTO.ImagesToRemove?
                .Where(url => !string.IsNullOrWhiteSpace(url))
                .ToList();

            // Verificar que se haya proporcionado al menos un campo para actualizar
            if (string.IsNullOrWhiteSpace(updateProductDTO.Name) &&
                string.IsNullOrWhiteSpace(updateProductDTO.Description) &&
                string.IsNullOrWhiteSpace(updateProductDTO.CategoryName) &&
                string.IsNullOrWhiteSpace(updateProductDTO.BrandName) &&
                !updateProductDTO.Price.HasValue &&
                !updateProductDTO.Stock.HasValue &&
                (updateProductDTO.ImagesToAdd == null || updateProductDTO.ImagesToAdd.Count == 0) &&
                (updateProductDTO.ImagesToRemove == null || updateProductDTO.ImagesToRemove.Count == 0))
            {
                throw new ArgumentException("Debes proporcionar al menos un campo para actualizar.");
            }

            // Verificar si el producto existe
            var product = await _productRepository.GetProductByIdForAdminAsync(id);
            if (product == null)
            {
                Log.Error("Producto no encontrado con ID: {ProductId}", id);
                throw new KeyNotFoundException("Producto no encontrado con ID: " + id);
            }

            // Verificar si los nuevos valores son iguales a los actuales
            var nothingChanged =
                (string.IsNullOrWhiteSpace(updateProductDTO.Name) || updateProductDTO.Name == product.Name.ToLower()) &&
                (string.IsNullOrWhiteSpace(updateProductDTO.Description) || updateProductDTO.Description == product.Description.ToLower()) &&
                (string.IsNullOrWhiteSpace(updateProductDTO.CategoryName) || updateProductDTO.CategoryName == product.Category.Name.ToLower()) &&
                (string.IsNullOrWhiteSpace(updateProductDTO.BrandName) || updateProductDTO.BrandName == product.Brand.Name.ToLower()) &&
                (!updateProductDTO.Price.HasValue || updateProductDTO.Price.Value == product.Price) &&
                (!updateProductDTO.Stock.HasValue || updateProductDTO.Stock.Value == product.Stock) &&
                (updateProductDTO.ImagesToAdd == null || updateProductDTO.ImagesToAdd.Count == 0) &&
                (updateProductDTO.ImagesToRemove == null || updateProductDTO.ImagesToRemove.Count == 0);

            if (nothingChanged)
                throw new ArgumentException("Los valores proporcionados son iguales a los actuales. No hay cambios que guardar.");

            // Determinar los nuevos valores para el nombre y la marca
            var newName = !string.IsNullOrWhiteSpace(updateProductDTO.Name) && updateProductDTO.Name != product.Name.ToLower()
                ? updateProductDTO.Name
                : product.Name.ToLower();

            var newBrandName = !string.IsNullOrWhiteSpace(updateProductDTO.BrandName) && updateProductDTO.BrandName != product.Brand.Name.ToLower()
                ? updateProductDTO.BrandName
                : product.Brand.Name.ToLower();

            // En caso de que el nombre o la marca hayan cambiado
            if (newName != product.Name.ToLower() || newBrandName != product.Brand.Name.ToLower())
            {
                // Verificar si la nueva marca existe (solo si cambió)
                if (newBrandName != product.Brand.Name.ToLower())
                {
                    var brandExists = await _brandRepository.ExistsByNameAsync(newBrandName);
                    if (!brandExists)
                    {
                        Log.Error("La marca no existe: {BrandName}", newBrandName);
                        throw new Exception($"La marca no existe: {newBrandName}");
                    }
                }

                // Verificar si ya existe otro producto con el mismo nombre y marca
                var productExists = await _productRepository.ExistsByNameAndBrandAsync(newName, newBrandName);
                if (productExists)
                {
                    Log.Error("Ya existe un producto con el mismo nombre y marca: {Name} - {Brand}", newName, newBrandName);
                    throw new Exception($"Ya existe un producto con el mismo nombre y marca: {newName} - {newBrandName}");
                }

                // Asignar valores
                // Actualizar el nombre si es diferente al actual
                if (newName != product.Name.ToLower())
                    product.Name = newName;

                // Actualizar la marca si es diferente al actual
                if (newBrandName != product.Brand.Name.ToLower())
                    product.BrandId = await _brandRepository.GetIdByNameAsync(newBrandName);
            }

            // Solo actualizar la categoría si se proporciona un nuevo valor y es diferente a la actual
            if (!string.IsNullOrEmpty(updateProductDTO.CategoryName) && updateProductDTO.CategoryName != product.Category.Name.ToLower())
            {
                // Verificar si la categoría existe
                var categoryExists = await _categoryRepository.ExistsByNameAsync(updateProductDTO.CategoryName);
                if (!categoryExists)
                {
                    Log.Error("La categoría no existe: {CategoryName}", updateProductDTO.CategoryName);
                    throw new Exception("La categoría no existe: " + updateProductDTO.CategoryName);
                }

                // Asignar el nuevo ID de categoría al producto
                product.CategoryId = await _categoryRepository.GetIdByNameAsync(updateProductDTO.CategoryName);
            }

            // Solo actualizar la descripción si se proporciona un nuevo valor y es diferente a la actual
            if (!string.IsNullOrWhiteSpace(updateProductDTO.Description) && updateProductDTO.Description != product.Description.ToLower())
                product.Description = updateProductDTO.Description;

            // Solo actualizar el precio si se proporciona un nuevo valor y es diferente al actual
            if (updateProductDTO.Price.HasValue && updateProductDTO.Price.Value != product.Price)
                product.Price = updateProductDTO.Price.Value;

            // Solo actualizar el stock si se proporciona un nuevo valor y es diferente al actual
            if (updateProductDTO.Stock.HasValue && updateProductDTO.Stock.Value != product.Stock)
                product.Stock = updateProductDTO.Stock.Value;

            // Actualizar el producto en la base de datos
            var isUpdated = await _productRepository.UpdateAsync(product);
            if (!isUpdated)
            {
                Log.Error("Error al actualizar el producto con ID: {ProductId}", id);
                throw new Exception("Error al actualizar el producto con ID: " + id);
            }


            // Eliminar las imágenes seleccionadas para eliminación
            if (updateProductDTO.ImagesToRemove != null && updateProductDTO.ImagesToRemove.Count > 0)
            {
                foreach (var imageUrl in updateProductDTO.ImagesToRemove)
                {
                    var imageToDelete = product.Images.FirstOrDefault(img => img.ImageUrl == imageUrl);
                    if (imageToDelete != null)
                    {
                        await ImageService.DeleteAsync(imageToDelete.PublicId);
                    }
                    else
                    {
                        Log.Warning("No se encontró la imagen para eliminar con URL: {ImageUrl} en el producto con ID: {ProductId}", imageUrl, id);
                    }
                }
            }

            // Subir las nuevas imágenes asociadas al producto
            if (updateProductDTO.ImagesToAdd != null && updateProductDTO.ImagesToAdd.Count > 0)
            {
                foreach (var image in updateProductDTO.ImagesToAdd)
                {
                    Log.Information("Nueva imagen asociada al producto: {@Image}", image);
                    await ImageService.UploadAsync(image, id);
                }
            }
        }
    }
}

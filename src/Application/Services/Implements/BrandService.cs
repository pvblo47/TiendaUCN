using Mapster;
using Serilog;
using TiendaUCN.src.Application.DTOs.BrandCategoryDTO;
using TiendaUCN.src.Application.Services.Interfaces;
using TiendaUCN.src.Domain.Models;
using TiendaUCN.src.Infrastructure.Repositories.Interfaces;

namespace TiendaUCN.src.Application.Services.Implements
{
    public class BrandService : IBrandService
    {
        private readonly IBrandRepository _brandRepository;

        public BrandService(IBrandRepository brandRepository)
        {
            _brandRepository = brandRepository;
        }

        public async Task<string> CreateBrandAsync(CreateBrandCategoryDTO brandDto)
        {
            var brandExists = await _brandRepository.ExistsByNameAsync(brandDto.Name);
            if (brandExists)
            {
                Log.Warning("Intento de crear una marca con un nombre ya existente: {BrandName}", brandDto.Name);
                throw new InvalidOperationException($"Ya existe una marca con el nombre '{brandDto.Name}'.");
            }

            // Crear una nueva instancia de Brand
            var brand = brandDto.Adapt<Brand>();

            // Guardar la marca en el repositorio, no es necesario crear un mapeo específico
            var isCreated = await _brandRepository.CreateAsync(brand);
            if (!isCreated)
            {
                Log.Error("Error al crear la marca: {BrandName}", brand.Name);
                throw new InvalidOperationException("No se pudo crear la marca.");
            }

            return $"Marca '{brand.Name}' creada exitosamente.";
        }

        public async Task<string> UpdateBrandAsync(int brandId, UpdateBrandCategoryDTO brandDto)
        {
            // Verificar la marca por su ID
            var brandExists = await _brandRepository.ExistsByIdAsync(brandId);
            if (!brandExists)
            {
                Log.Warning("Marca con ID {BrandId} no encontrada para actualización.", brandId);
                throw new KeyNotFoundException($"Marca con ID {brandId} no encontrada.");
            }

            // En caso de que se no se proporcionen datos para actualizar
            if (string.IsNullOrEmpty(brandDto.Name) && string.IsNullOrEmpty(brandDto.Description))
            {
                Log.Warning("Intento de actualizar la marca con datos vacíos.", brandId);
                throw new InvalidOperationException("Debe proporcionar al menos un nombre o una descripción para actualizar la marca.");
            }

            // En caso de que se quiera actualizar solo el nombre
            else if (!string.IsNullOrEmpty(brandDto.Name) && string.IsNullOrEmpty(brandDto.Description))
            {
                // Verificar si el nuevo nombre ya existe en otra marca
                var nameExists = await _brandRepository.ExistsByNameAsync(brandDto.Name);
                if (nameExists)
                {
                    Log.Warning("Intento de actualizar la marca con un nombre ya existente: {BrandName}", brandDto.Name);
                    throw new InvalidOperationException($"Ya existe una marca con el nombre '{brandDto.Name}'.");
                }

                // Actualizar solo el nombre de la marca
                var isNameUpdated = await _brandRepository.UpdateNameAsync(brandId, brandDto.Name);
                if (!isNameUpdated)
                {
                    Log.Error("Error al actualizar el nombre de la marca con ID {BrandId}.", brandId);
                    throw new InvalidOperationException("No se pudo actualizar el nombre de la marca.");
                }

                return $"Nombre de la marca con ID {brandId} actualizado exitosamente a '{brandDto.Name}'.";
            }

            // En caso de que se quiera actualizar solo la descripción            
            else if (string.IsNullOrEmpty(brandDto.Name) && !string.IsNullOrEmpty(brandDto.Description))
            {
                // Actualizar solo la descripción de la marca
                var isDescriptionUpdated = await _brandRepository.UpdateDescriptionAsync(brandId, brandDto.Description);
                if (!isDescriptionUpdated)
                {
                    Log.Error("Error al actualizar la descripción de la marca con ID {BrandId}.", brandId);
                    throw new InvalidOperationException("No se pudo actualizar la descripción de la marca.");
                }

                return $"Descripción de la marca con ID {brandId} actualizada exitosamente.";
            }

            // En caso de que se quieran actualizar ambos campos
            else
            {
                // Verificar si el nuevo nombre ya existe en otra marca
                var nameExists = await _brandRepository.ExistsByNameAsync(brandDto.Name!);
                if (nameExists)
                {
                    Log.Warning("Intento de actualizar la marca con un nombre ya existente: {BrandName}", brandDto.Name);
                    throw new InvalidOperationException($"Ya existe una marca con el nombre '{brandDto.Name}'.");
                }

                // Actualizar ambos campos de la marca
                var isNameUpdated = await _brandRepository.UpdateNameAsync(brandId, brandDto.Name!);
                var isDescriptionUpdated = await _brandRepository.UpdateDescriptionAsync(brandId, brandDto.Description!);

                if (!isNameUpdated || !isDescriptionUpdated)
                {
                    Log.Error("Error al actualizar la marca con ID {BrandId}.", brandId);
                    throw new InvalidOperationException("No se pudo actualizar la marca.");
                }

                return $"Marca con ID {brandId} actualizada exitosamente a Nombre: '{brandDto.Name}' y Descripción: '{brandDto.Description}'.";
            }
        }

        public async Task<string> DeleteBrandAsync(int brandId)
        {
            // Verificar la marca por su ID
            var brandExists = await _brandRepository.ExistsByIdAsync(brandId);
            if (!brandExists)
            {
                Log.Warning("Marca con ID {BrandId} no encontrada para eliminación.", brandId);
                throw new KeyNotFoundException($"Marca con ID {brandId} no encontrada.");
            }

            // Eliminar la marca del repositorio
            var isDeleted = await _brandRepository.DeleteAsync(brandId);
            if (!isDeleted)
            {
                Log.Error("Error al eliminar la marca con ID {BrandId}.", brandId);
                throw new InvalidOperationException("No se pudo eliminar la marca.");
            }

            return $"Marca con ID {brandId} eliminada exitosamente.";
        }

        public async Task<List<CatalogItemDTO>> GetAllActiveBrandsAsync()
        {
            var brands = await _brandRepository.GetAllActiveAsync();
            return brands.Adapt<List<CatalogItemDTO>>();
        }
    }
}
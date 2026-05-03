using Mapster;
using Serilog;
using TiendaUCN.src.Application.DTOs.BrandCategoryDTO;
using TiendaUCN.src.Application.Services.Interfaces;
using TiendaUCN.src.Domain.Models;
using TiendaUCN.src.Infrastructure.Repositories.Interfaces;

namespace TiendaUCN.src.Application.Services.Implements
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<string> CreateCategoryAsync(CreateBrandCategoryDTO categoryDto)
        {
            var categoryExists = await _categoryRepository.ExistsByNameAsync(categoryDto.Name);
            if (categoryExists)
            {
                Log.Warning("Intento de crear una categoría con un nombre ya existente: {CategoryName}", categoryDto.Name);
                throw new InvalidOperationException($"Ya existe una categoría con el nombre '{categoryDto.Name}'.");
            }

            // Crear una nueva instancia de Category, no es necesario crear un mapeo específico
            var category = categoryDto.Adapt<Category>();

            // Guardar la categoría en el repositorio
            var isCreated = await _categoryRepository.CreateAsync(category);
            if (!isCreated)
            {
                Log.Error("Error al crear la categoría: {CategoryName}", category.Name);
                throw new InvalidOperationException("No se pudo crear la categoría.");
            }

            return $"Categoría '{category.Name}' creada exitosamente.";
        }

        public async Task<string> UpdateCategoryAsync(int categoryId, UpdateBrandCategoryDTO categoryDto)
        {
            // Verificar la categoría por su ID
            var categoryExists = await _categoryRepository.ExistsByIdAsync(categoryId);
            if (!categoryExists)
            {
                Log.Warning("Categoría con ID {CategoryId} no encontrada para actualización.", categoryId);
                throw new KeyNotFoundException($"Categoría con ID {categoryId} no encontrada.");
            }

            // En caso de que se no se proporcionen datos para actualizar
            if (string.IsNullOrEmpty(categoryDto.Name) && string.IsNullOrEmpty(categoryDto.Description))
            {
                Log.Warning("Intento de actualizar la categoría con datos vacíos.", categoryId);
                throw new InvalidOperationException("Debe proporcionar al menos un nombre o una descripción para actualizar la categoría.");
            }

            // En caso de que se quiera actualizar solo el nombre
            else if (!string.IsNullOrEmpty(categoryDto.Name) && string.IsNullOrEmpty(categoryDto.Description))
            {
                // Verificar si el nuevo nombre ya existe en otra categoría
                var nameExists = await _categoryRepository.ExistsByNameAsync(categoryDto.Name);
                if (nameExists)
                {
                    Log.Warning("Intento de actualizar la categoría con un nombre ya existente: {CategoryName}", categoryDto.Name);
                    throw new InvalidOperationException($"Ya existe una categoría con el nombre '{categoryDto.Name}'.");
                }

                // Actualizar solo el nombre de la categoría
                var isNameUpdated = await _categoryRepository.UpdateNameAsync(categoryId, categoryDto.Name);
                if (!isNameUpdated)
                {
                    Log.Error("Error al actualizar el nombre de la categoría con ID {CategoryId}.", categoryId);
                    throw new InvalidOperationException("No se pudo actualizar el nombre de la categoría.");
                }

                return $"Nombre de la categoría con ID {categoryId} actualizado exitosamente a '{categoryDto.Name}'.";
            }

            // En caso de que se quiera actualizar solo la descripción            
            else if (string.IsNullOrEmpty(categoryDto.Name) && !string.IsNullOrEmpty(categoryDto.Description))
            {
                // Actualizar solo la descripción de la categoría
                var isDescriptionUpdated = await _categoryRepository.UpdateDescriptionAsync(categoryId, categoryDto.Description);
                if (!isDescriptionUpdated)
                {
                    Log.Error("Error al actualizar la descripción de la categoría con ID {CategoryId}.", categoryId);
                    throw new InvalidOperationException("No se pudo actualizar la descripción de la categoría.");
                }

                return $"Descripción de la categoría con ID {categoryId} actualizada exitosamente.";
            }

            // En caso de que se quieran actualizar ambos campos
            else
            {
                // Verificar si el nuevo nombre ya existe en otra categoría
                var nameExists = await _categoryRepository.ExistsByNameAsync(categoryDto.Name!);
                if (nameExists)
                {
                    Log.Warning("Intento de actualizar la categoría con un nombre ya existente: {CategoryName}", categoryDto.Name);
                    throw new InvalidOperationException($"Ya existe una categoría con el nombre '{categoryDto.Name}'.");
                }

                // Actualizar ambos campos de la categoría
                var isNameUpdated = await _categoryRepository.UpdateNameAsync(categoryId, categoryDto.Name!);
                var isDescriptionUpdated = await _categoryRepository.UpdateDescriptionAsync(categoryId, categoryDto.Description!);

                if (!isNameUpdated || !isDescriptionUpdated)
                {
                    Log.Error("Error al actualizar la categoría con ID {CategoryId}.", categoryId);
                    throw new InvalidOperationException("No se pudo actualizar la categoría.");
                }

                return $"Categoría con ID {categoryId} actualizada exitosamente a Nombre: '{categoryDto.Name}' y Descripción: '{categoryDto.Description}'.";
            }
        }

        public async Task<string> DeleteCategoryAsync(int categoryId)
        {
            // Verificar la categoría por su ID
            var categoryExists = await _categoryRepository.ExistsByIdAsync(categoryId);
            if (!categoryExists)
            {
                Log.Warning("Categoría con ID {CategoryId} no encontrada para eliminación.", categoryId);
                throw new KeyNotFoundException($"Categoría con ID {categoryId} no encontrada.");
            }

            // Eliminar la categoría del repositorio
            var isDeleted = await _categoryRepository.DeleteAsync(categoryId);
            if (!isDeleted)
            {
                Log.Error("Error al eliminar la categoría con ID {CategoryId}.", categoryId);
                throw new InvalidOperationException("No se pudo eliminar la categoría.");
            }

            return $"Categoría con ID {categoryId} eliminada exitosamente.";
        }
    }
}
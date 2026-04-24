using Microsoft.EntityFrameworkCore;
using TiendaUCN.src.Domain.Models;
using TiendaUCN.src.Infrastructure.Data;
using TiendaUCN.src.Infrastructure.Repositories.Interfaces;

namespace TiendaUCN.src.Infrastructure.Repositories.Implements
{
    public class ImageRepository : IImageRepository
    {
        private readonly DataContext _context;
        public ImageRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<bool?> CreateAsync(Image image)
        {
            var existsImage = await _context.Images.AnyAsync(i => i.PublicId == image.PublicId);
            if (!existsImage)
            {
                _context.Images.Add(image);

                // Si la imagen se agrega correctamente, retorna true, de lo contrario false
                return await _context.SaveChangesAsync() > 0;
            }

            // Si la image ya existe, retrorna null
            return null;
        }

        public async Task<bool?> DeleteAsync(string publicId)
        {
            var image = await _context.Images.FirstOrDefaultAsync(i => i.PublicId == publicId);
            if (image != null)
            {
                _context.Images.Remove(image);

                // Si la imagen se elimina correctamente, retorna true, de lo contrario false
                return await _context.SaveChangesAsync() > 0;
            }

            // Si la imagen no existe, retrorna null
            return null;
        }
    }
}
namespace TiendaUCN.src.Application.Services.Interfaces
{
    public interface IImageService
    {
        Task<bool> UploadAsync(IFormFile file, int productId);
        Task<bool> DeleteAsync(string publicId);
    }
}
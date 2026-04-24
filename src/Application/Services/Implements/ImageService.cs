using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Serilog;
using SkiaSharp;
using TiendaUCN.src.Application.Services.Interfaces;
using TiendaUCN.src.Domain.Models;
using TiendaUCN.src.Infrastructure.Repositories.Interfaces;

namespace TiendaUCN.src.Application.Services.Implements
{
    public class ImageService : IImageService
    {
        private readonly IConfiguration _configuration;
        private readonly Cloudinary _cloudinary;
        private readonly string[] _allowedExtensions;
        private readonly int _maxFileSizeInBytes;

        private readonly IImageRepository _imageRepository;
        private readonly string _cloudName;
        private readonly string _cloudApiKey;
        private readonly string _cloudApiSecret;
        private readonly int _transformationWidth;
        private readonly string _transformationCrop;
        private readonly string _transformationQuality;
        private readonly string _transformationFetchFormat;
        public ImageService(IConfiguration configuration, IImageRepository imageRepository)
        {
            _configuration = configuration;
            _imageRepository = imageRepository;
            _cloudName = Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME") ?? throw new InvalidOperationException("La configuración de CloudName es obligatoria");
            _cloudApiKey = Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY") ?? throw new InvalidOperationException("La configuración de ApiKey es obligatoria");
            _cloudApiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET") ?? throw new InvalidOperationException("La configuración de ApiSecret es obligatoria");
            Account account = new Account(_cloudName, _cloudApiKey, _cloudApiSecret);
            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true; // Aseguramos  que las URLs sean seguras con HTTPS
            _allowedExtensions = _configuration.GetSection("Products:ImageAllowedExtensions").Get<string[]>() ?? throw new InvalidOperationException("La configuración de las extensiones de las imágenes es obligatoria");
            _transformationQuality = _configuration["Products:TransformationQuality"] ?? throw new InvalidOperationException("La configuración de la calidad de la transformación es obligatoria");
            _transformationCrop = _configuration["Products:TransformationCrop"] ?? throw new InvalidOperationException("La configuración del recorte de la transformación es obligatoria");
            _transformationFetchFormat = _configuration["Products:TransformationFetchFormat"] ?? throw new InvalidOperationException("La configuración del formato de la transformación es obligatoria");
            if (!int.TryParse(_configuration["Products:ImageMaxSizeInBytes"], out _maxFileSizeInBytes)) { throw new InvalidOperationException("La configuración del tamaño de la imagen es obligatoria"); }
            if (!int.TryParse(_configuration["Products:TransformationWidth"], out _transformationWidth)) { throw new InvalidOperationException("La configuración del ancho de la transformación es obligatoria"); }
        }
        public async Task<bool> UploadAsync(IFormFile file, int productId)
        {
            // Validar el productId
            if (productId <= 0)
            {
                Log.Error($"ProductId inválido: {productId}");
                throw new ArgumentException("ProductId debe ser mayor a 0");
            }

            // Validar el archivo
            if (file == null || file.Length == 0)
            {
                Log.Error("Intento de subir un archivo nulo o vacío");
                throw new ArgumentException("Archivo inválido");
            }

            // Validar el tamaño del archivo
            if (file.Length > _maxFileSizeInBytes)
            {
                Log.Error($"El archivo {file.FileName} excede el tamaño máximo permitido de {_maxFileSizeInBytes / 1024 / 1024} MB");
                throw new ArgumentException($"El archivo excede el tamaño máximo permitido de {_maxFileSizeInBytes / 1024 / 1024} MB");
            }

            // Validar la extensión del archivo
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!_allowedExtensions.Contains(fileExtension))
            {
                Log.Error($"Extensión de archivo no permitida: {fileExtension}");
                throw new ArgumentException($"Extensión de archivo no permitida. Extensiones permitidas: {string.Join(", ", _allowedExtensions)}");
            }

            // Validar que el archivo sea una imagen real
            if (!IsValidImageFile(file))
            {
                Log.Error($"El archivo {file.FileName} no es una imagen válida");
                throw new ArgumentException("El archivo no es una imagen válida");
            }

            // Subir la imagen a Cloudinary
            var folder = $"product/{productId}/images";

            using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams()
            {
                Folder = folder,
                File = new FileDescription(file.FileName, stream),
                UseFilename = true, // Mantener el nombre original del archivo
                UniqueFilename = true // Asegurar que el nombre del archivo sea único
            };

            // Aplicar transformaciones para optimizar la imagen
            Log.Information($"Optimizando imagen: {file.FileName} antes de subir a la nube");
            uploadParams.Transformation = new Transformation()
                .Width(_transformationWidth).Crop(_transformationCrop).Chain()
                .Quality(_transformationQuality).Chain()
                .FetchFormat(_transformationFetchFormat);

            // Subir la imagen a Cloudinary
            Log.Information($"Subiendo imagen: {file.FileName} a Cloudinary");
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            if (uploadResult.Error != null)
            {
                Log.Error($"Hubo un error al subir la imagen: {uploadResult.Error.Message}");
                throw new Exception($"Error al subir la imagen: {uploadResult.Error.Message}");
            }

            var image = new Image()
            {
                PublicId = uploadResult.PublicId,
                ImageUrl = uploadResult.SecureUrl.ToString(),
                ProductId = productId
            };

            // Guardar la información de la imagen en la base de datos
            var result = await _imageRepository.CreateAsync(image);
            if (result is bool && !result.Value!)
            {
                Log.Error($"Error al guardar la imagen en la base de datos: {file.FileName}");
                var deleteResult = await DeleteInCloudinaryAsync(uploadResult.PublicId); // Eliminamos la imagen de Cloudinary si falla la creación de la imagen en la bdd
                if (!deleteResult)
                {
                    Log.Error($"Error al eliminar la imagen de Cloudinary después de fallar la creación en la base de datos: {uploadResult.PublicId}");
                    throw new Exception("Error al eliminar la imagen de Cloudinary después de fallar la creación en la base de datos");
                }
                throw new Exception("Error al guardar la imagen en la base de datos");
            }
            else if (result is null)
            {
                Log.Warning($"La imagen ya existe en la base de datos: {file.FileName}");
                return false;
            }

            Log.Information($"Imagen subida exitosamente: {uploadResult.SecureUrl}");
            return true;
        }

        public async Task<bool> DeleteAsync(string publicId)
        {

            // Eliminar la imagen de Cloudinary
            var deletionParams = new DeletionParams(publicId);
            Log.Information($"Eliminando imagen con PublicId: {publicId} de Cloudinary");
            var deleteResult = await _cloudinary.DestroyAsync(deletionParams);
            if (deleteResult.Error != null)
            {
                Log.Error($"Error al eliminar la imagen con PublicId: {publicId} de Cloudinary: {deleteResult.Error.Message}");
                throw new Exception($"Error al eliminar la imagen: {deleteResult.Error.Message}");
            }
            Log.Information($"Imagen con PublicId: {publicId} eliminada exitosamente de Cloudinary");

            // Eliminar la información de la imagen de la base de datos
            var result = await _imageRepository.DeleteAsync(publicId);
            if (result is bool && !result.Value!)
            {
                Log.Error($"Error al eliminar la imagen de la base de datos con PublicId: {publicId}");
                throw new Exception("Error al eliminar la imagen de la base de datos");
            }
            else if (result is null)
            {
                Log.Warning($"La imagen no existe en la base de datos con PublicId: {publicId}");
                return false;
            }
            Log.Information($"Imagen con PublicId: {publicId} eliminada exitosamente de la base de datos");
            return true;
        }

        private async Task<bool> DeleteInCloudinaryAsync(string publicId)
        {
            var deletionParams = new DeletionParams(publicId);
            Log.Information($"Eliminando imagen con PublicId: {publicId} de Cloudinary");
            var deleteResult = await _cloudinary.DestroyAsync(deletionParams);
            if (deleteResult.Error != null)
            {
                Log.Error($"Error al eliminar la imagen con PublicId: {publicId} de Cloudinary: {deleteResult.Error.Message}");
                return false;
            }
            Log.Information($"Imagen con PublicId: {publicId} eliminada exitosamente de Cloudinary");
            return true;
        }

        private bool IsValidImageFile(IFormFile file)
        {
            try
            {
                using var stream = file.OpenReadStream();

                // Usamos SkiaSharp para intentar decodificar la imagen y verificar que sea válida
                using var skiaStream = new SKManagedStream(stream);

                // Crea un codec para leer la imagen (codificador/decodificador)
                using var codec = SKCodec.Create(skiaStream);

                // Si el codec es nulo, no es una imagen válida. Además, verificamos que las dimensiones sean mayores a 0.
                return codec != null && codec.Info.Width > 0 && codec.Info.Height > 0;
            }
            catch (Exception ex)
            {
                Log.Warning($"Error validando imagen {file.FileName}: {ex.Message}");
                return false;
            }
        }
    }
}
using Serilog;
using System.Security;
using System.Text.Json;
using TiendaUCN.src.Application.DTOs.BaseResponse;

namespace TiendaUCN.src.API.Middlewares
{
    public class ExceptionHandlingMiddleware(RequestDelegate next)
    {
        // Punto de entrada del middleware, recibe el siguiente delegado en la cadena de middlewares
        private readonly RequestDelegate next = next;

        // Metodo que ejecuta automaticamente el middleware para cada solicitud HTTP
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Intentea ejecutar el siguiente Middleware en la cadena
                // si no ocurre ninguna excepción, la solicitud se procesa normalmente
                await next(context);
            }
            catch (Exception ex)
            {
                // Genera un ID de traza único para esta excepción
                var traceId = Guid.NewGuid().ToString();
                // Lo agrega a los encabezados de la respuesta
                context.Response.Headers["trace-id"] = traceId;

                // Mapea la excepción a un código de estado HTTP y un título descriptivo
                var (statusCode, title) = MapExceptionToStatus(ex);

                // Crea un objeto de error con el título y el mensaje de la excepción
                ErrorDetail error = new ErrorDetail(title, ex.Message);

                Log.Error(ex, "Excepción no controlada. Trace ID: {TraceId}", traceId);

                // Define que la respuesta será JSON y establece el código de estado HTTP
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = statusCode;

                // Serializa el objeto de error a JSON con formato camelCase
                var json = JsonSerializer.Serialize(
                    error,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
                );

                // Escribe el JSON de error en la respuesta HTTP
                await context.Response.WriteAsync(json);
            }
        }

        private static (int, string) MapExceptionToStatus(Exception ex)
        {
            return ex switch
            {
                // Mapea diferentes tipos de excepciones a códigos de estado HTTP y títulos descriptivos
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "No autorizado"),
                ArgumentNullException => (StatusCodes.Status400BadRequest, "Solicitud inválida"),
                KeyNotFoundException => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
                InvalidOperationException => (StatusCodes.Status409Conflict, "Conflicto de operación"),
                FormatException => (StatusCodes.Status400BadRequest, "Formato inválido"),
                SecurityException => (StatusCodes.Status403Forbidden, "Acceso prohibido"),
                ArgumentOutOfRangeException => (StatusCodes.Status400BadRequest, "Argumento fuera de rango"),
                ArgumentException => (StatusCodes.Status400BadRequest, "Argumento invalido"),
                TimeoutException => (StatusCodes.Status429TooManyRequests, "Demasiadas solicitudes"),
                JsonException => (StatusCodes.Status400BadRequest, "JSON inválido"),
                _ => (StatusCodes.Status500InternalServerError, "Error interno del servidor")
            };
        }
    }
}
using System.Text.Json;
using TiendaUCN.src.Application.DTOs.BaseResponse;
using TiendaUCN.src.Application.Services.Interfaces;

namespace TiendaUCN.src.API.Middlewares
{
    public class BlacklistMiddleware(RequestDelegate next)
    {
        // Punto de entrada del Middleware. Recibe el siguiente delegado en la cadena de solicitud.
        private readonly RequestDelegate _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            // Obtén el servicio ITokenService desde el contenedor de servicios por solicitud.
            var tokenService = context.RequestServices.GetService<ITokenService>();

            if (tokenService == null)
            {
                // Manejo en caso de que no se resuelva el servicio.
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";
                var json = JsonSerializer.Serialize(
                    new ErrorDetail("Error interno del servidor", "No se pudo resolver el servicio de tokens."),
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
                );
                await context.Response.WriteAsync(json);
                return;
            }

            // Verifica si hay un token en los encabezados de la solicitud.
            if (context.Request.Headers.TryGetValue("Authorization", out var token))
            {
                // Extrae el token.
                var tokenValue = token.ToString().Replace("Bearer ", string.Empty);

                // Verifica si el token está en la lista negra.
                if (await tokenService.IsTokenBlacklistedAsync(tokenValue))
                {
                    // Si el token está en la lista negra, devuelve un código de estado 401 Unauthorized.
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;

                    // Devuelve un mensaje de error en formato JSON.
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    var json = JsonSerializer.Serialize(
                        new ErrorDetail("No autorizado", "El token ha sido invalidado."),
                        new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
                    );
                    await context.Response.WriteAsync(json);
                    return;
                }
            }

            // Llama al siguiente middleware a ejecutarse en el sistema.
            await _next(context);
        }
    }
}
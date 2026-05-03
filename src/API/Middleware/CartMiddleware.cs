using Serilog;

namespace TiendaUCN.src.API.Middlewares
{
    public class CartMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly int _cookieExpirationDays;

        public CartMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
            _cookieExpirationDays = _configuration.GetValue<int?>("CookieExpirationDays") ?? throw new ArgumentNullException("La expiración en días de la cookie no está configurada.");
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Verificamos si el cliente ya tiene una cookie de comprador (BuyerId)
            var buyerId = context.Request.Cookies["BuyerId"];

            // Si no existe la cookie, generamos un nuevo buyerId
            if (string.IsNullOrEmpty(buyerId))
            {
                Log.Information("No se encontró la cookie de comprador, creando una nueva.");

                // Generamos un nuevo buyerId utilizando UUIDv7 para garantizar unicidad y orden temporal
                buyerId = Guid.CreateVersion7().ToString();
                Log.Information("Se creó una nueva cookie de comprador: {BuyerId}", buyerId);
            }

            // Se renueva la cookie con cada solicitud para extender su vida útil
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Asegura que la cookie solo se envíe a través de HTTPS
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(_cookieExpirationDays),
                Path = "/", // Las cookies serán accesibles desde cualquier ruta
            };

            // Asignamos el buyerId a la cookie de respuesta
            context.Response.Cookies.Append("BuyerId", buyerId, cookieOptions);

            // Almacenamos el buyerId en el contexto para que esté disponible durante la solicitud
            context.Items["BuyerId"] = buyerId;

            await _next(context);
        }
    }
}
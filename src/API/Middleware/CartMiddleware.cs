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
            _cookieExpirationDays = _configuration.GetValue<int?>("CookieExpirationDays")
                ?? throw new ArgumentNullException("La expiracion en dias de la cookie no esta configurada.");
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var buyerId = context.Request.Cookies["BuyerId"];

            if (string.IsNullOrEmpty(buyerId))
            {
                Log.Information("No se encontro la cookie de comprador, creando una nueva.");

                buyerId = Guid.CreateVersion7().ToString();
                Log.Information("Se creo una nueva cookie de comprador: {BuyerId}", buyerId);
            }

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = context.Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(_cookieExpirationDays),
                Path = "/",
            };

            context.Response.Cookies.Append("BuyerId", buyerId, cookieOptions);
            context.Items["BuyerId"] = buyerId;

            await _next(context);
        }
    }
}

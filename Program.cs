using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using Serilog;
using Resend;
using TiendaUCN.src.Infrastructure.Data;
using TiendaUCN.src.Application.Services.Interfaces;
using TiendaUCN.src.Application.Services.Implements;
using TiendaUCN.src.Infrastructure.Repositories.Interfaces;
using TiendaUCN.src.Infrastructure.Repositories.Implements;
using TiendaUCN.src.API.Middlewares;
using DotNetEnv;
using TiendaUCN.src.Application.Mappers;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();

//Configuracion de mapeadores
builder.Services.AddScoped<UserMapper>();

//Configuracion de servicios y repositorios
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ITokenRepository, TokenRepository>();

#region Loggin Configuration
Serilog.Log.Information("Configurando Serilog para loggin");
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services));
#endregion

#region Database Configuration
Serilog.Log.Information("Configurando base de datos SQLite");
string connectionStringDB = Environment.GetEnvironmentVariable("DATA_BASE_URL")
?? throw new ArgumentNullException("DataBase name cannot be null or empty");

builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlite(connectionStringDB));
#endregion

#region Email Service Configuration
Serilog.Log.Information("Configurando servicio de correo electrónico Resend");
builder.Services.AddOptions();
builder.Services.AddHttpClient<ResendClient>();
builder.Services.Configure<ResendClientOptions>(o =>
{
    o.ApiToken = Environment.GetEnvironmentVariable("RESEND_API_KEY") ?? throw new ArgumentNullException("RESEND_API_KEY is not set");
});
builder.Services.AddTransient<IResend, ResendClient>();
#endregion

#region Authentication Configuration
Serilog.Log.Information("Configurando autenticación JWT");
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }
    ).AddJwtBearer(options =>
    {
        string jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? throw new InvalidOperationException("La clave secreta JWT no esta configurada");
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateLifetime = true, // Valida la expiración del token
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero //Sin tolerencia a tokens expirados
        };
    });
#endregion


var app = builder.Build();

#region Database Migration
Serilog.Log.Information("Aplicando migraciones a la base de datos");
using (var scope = app.Services.CreateScope())
{
    await DataSeeder.Initialize(scope.ServiceProvider);

    // Configurar los mapeos de Mapster
    MapperExtensions.ConfigureMapster(scope.ServiceProvider);
}
#endregion

app.UseMiddleware<ExceptionHandlingMiddleware>(); // 1 - valida el JWT
app.UseMiddleware<BlacklistMiddleware>(); // 2 - verifica blacklist
app.UseAuthentication(); // 3 - verifica roles y permisos
app.UseAuthorization();
app.MapOpenApi();
app.MapControllers();
app.Run();
using Microsoft.EntityFrameworkCore;
using Serilog;
using Resend;
using TiendaUCN.src.Infrastructure.Data;
using TiendaUCN.src.Application.Services.Interfaces;
using TiendaUCN.src.Application.Services.Implements;
using TiendaUCN.src.Infrastructure.Repositories.Interfaces;
using TiendaUCN.src.Infrastructure.Repositories.Implements;
using DotNetEnv;

Env.Load();

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddOpenApi();

//Configuracion de mapeadores
//builder.Services.AddScoped<UserMapper>();

//Configuracion de servicios y repositorios
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ITokenRepository, TokenRepository>();

#region Loggin Configuration
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

var app = builder.Build();

#region Database Migration
Serilog.Log.Information("Aplicando migraciones a la base de datos");
using (var scope = app.Services.CreateScope())
{
    await DataSeeder.Initialize(scope.ServiceProvider);
}
#endregion

app.MapOpenApi();

app.Run();
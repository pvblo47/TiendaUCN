using DotNetEnv;
using Hangfire;
using Hangfire.Storage.SQLite;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Resend;
using Serilog;
using System.Text;
using TiendaUCN.src.API.Middlewares;
using TiendaUCN.src.Application.Jobs.Implements;
using TiendaUCN.src.Application.Jobs.Interfaces;
using TiendaUCN.src.Application.Mappers;
using TiendaUCN.src.Application.Services.Implements;
using TiendaUCN.src.Application.Services.Interfaces;
using TiendaUCN.src.Infrastructure.Data;
using TiendaUCN.src.Infrastructure.Repositories.Implements;
using TiendaUCN.src.Infrastructure.Repositories.Interfaces;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();

//Configuracion de mapeadores
builder.Services.AddScoped<UserMapper>();

//Configuracion de servicios y repositorios
builder.Services.AddScoped<IVerificationCodeRepository, VerificationCodeRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IImageRepository, ImageRepository>();
builder.Services.AddScoped<ITokenRepository, TokenRepository>();

// Configuración de trabajos en segundo plano
builder.Services.AddScoped<IUserJob, UserJob>();


#region Database Configuration
Serilog.Log.Information("Configurando base de datos SQLite");
string connectionStringDB = Environment.GetEnvironmentVariable("DATA_BASE_URL") ?? throw new ArgumentNullException("DATA_BASE_URL is not set");
builder.Services.AddDbContext<DataContext>(options => options.UseSqlite(connectionStringDB));
#endregion

#region Loggin Configuration
Serilog.Log.Information("Configurando Serilog para loggin");
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services));
#endregion

#region Hangfire Configuration
Serilog.Log.Information("Configurando los trabajos en segundo plano de Hangfire");
var cronExpressionDeleteUnconfirmedUsers = builder.Configuration["Jobs:CronJobDeleteUnconfirmedUsers"] ?? throw new InvalidOperationException("La expresión cron para eliminar usuarios no confirmados no está configurada.");
var cronExpressionDeleteExpiredTokens = builder.Configuration["Jobs:CronJobDeleteExpiredTokens"] ?? throw new InvalidOperationException("La expresión cron para eliminar tokens expirados no está configurada.");
var timeZone = TimeZoneInfo.FindSystemTimeZoneById(builder.Configuration["Jobs:TimeZone"] ?? throw new InvalidOperationException("La zona horaria para los trabajos no está configurada."));
builder.Services.AddHangfire(configuration =>
{
    var connectionStringBuilder = new SqliteConnectionStringBuilder(connectionStringDB);
    var databasePath = connectionStringBuilder.DataSource;

    configuration.UseSQLiteStorage(databasePath);
    configuration.SetDataCompatibilityLevel(CompatibilityLevel.Version_170);
    configuration.UseSimpleAssemblyNameTypeSerializer();
    configuration.UseRecommendedSerializerSettings();
});
builder.Services.AddHangfireServer();
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

#region Hangfire Dashboard Configuration
Serilog.Log.Information("Configurando el panel de control de Hangfire");
app.UseHangfireDashboard(builder.Configuration["HangfireDashboard:DashboardPath"] ?? throw new InvalidOperationException("La ruta de hangfire no ha sido declarada"), new DashboardOptions
{
    StatsPollingInterval = builder.Configuration.GetValue<int?>("HangfireDashboard:StatsPollingInterval") ?? throw new InvalidOperationException("El intervalo de actualización de estadísticas del panel de control de Hangfire no está configurado."),
    DashboardTitle = builder.Configuration["HangfireDashboard:DashboardTitle"] ?? throw new InvalidOperationException("El título del panel de control de Hangfire no está configurado."),
    DisplayStorageConnectionString = builder.Configuration.GetValue<bool?>("HangfireDashboard:DisplayStorageConnectionString") ?? throw new InvalidOperationException("La configuración 'HangfireDashboard:DisplayStorageConnectionString' no está definida."),
});
#endregion


#region Database Migration
Serilog.Log.Information("Aplicando migraciones a la base de datos");
using (var scope = app.Services.CreateScope())
{
    await DataSeeder.Initialize(scope.ServiceProvider);

    // Configurar los mapeos de Mapster
    MapperExtensions.ConfigureMapster(scope.ServiceProvider);
}
#endregion

# region Configuración de trabajos recurrentes de Hangfire
Serilog.Log.Information("Configurando trabajos recurrentes de Hangfire");
// Configurar el trabajo para eliminar los usuarios no confirmados
var jobId = nameof(UserJob.DeleteUnconfirmedUsersAsync);
RecurringJob.AddOrUpdate<UserJob>(
    jobId,
    job => job.DeleteUnconfirmedUsersAsync(),
    cronExpressionDeleteUnconfirmedUsers,
    new RecurringJobOptions
    {
        TimeZone = timeZone
    }
);
Serilog.Log.Information($"Job recurrente '{jobId}' configurado con cron: {cronExpressionDeleteUnconfirmedUsers} en zona horaria: {timeZone.Id}");

// Configurar el trabajo para eliminar los tokens expirados en la blacklist
jobId = nameof(UserJob.DeleteExpiredTokensInBlacklistAsync);
RecurringJob.AddOrUpdate<UserJob>(
    jobId,
    job => job.DeleteExpiredTokensInBlacklistAsync(),
    cronExpressionDeleteExpiredTokens,
    new RecurringJobOptions
    {
        TimeZone = timeZone
    }
);
Serilog.Log.Information($"Job recurrente '{jobId}' configurado con cron: {cronExpressionDeleteExpiredTokens} en zona horaria: {timeZone.Id}");
#endregion

app.UseMiddleware<ExceptionHandlingMiddleware>(); // 1 - maneja excepciones
app.UseAuthentication(); // 2 - valida el JWT
app.UseMiddleware<BlacklistMiddleware>(); // 3 - verifica blacklist
app.UseAuthorization(); // 4 - verifica roles y permisos
app.MapOpenApi();
app.MapControllers();
app.Run();
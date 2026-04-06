using Microsoft.EntityFrameworkCore;
using Serilog;
using TiendaUCN.src.Infrastructure.Data;
using DotNetEnv;

Env.Load();

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddOpenApi();

#region Loggin Configuration
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services));
#endregion

#region Database Configuration
Log.Information("Configurando base de datos SQLite");
string connectionStringDB = Environment.GetEnvironmentVariable("DATA_BASE_URL")
?? throw new ArgumentNullException("DataBase name cannot be null or empty");

builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlite(connectionStringDB));
#endregion

var app = builder.Build();

#region Database Migration
Log.Information("Aplicando migraciones a la base de datos");
using (var scope = app.Services.CreateScope())
{
    await DataSeeder.Initialize(scope.ServiceProvider);
}
#endregion

app.MapOpenApi();

app.Run();
// a
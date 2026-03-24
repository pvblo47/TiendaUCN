using Serilog;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddOpenApi();

#region Loggin Configuration
builder.Host.UseSerilog((context, services,configuration) =>configuration.ReadFrom.Configuration(context.Configuration).ReadFrom.Services(services));

#endregion
var app = builder.Build();

app.MapOpenApi();

app.Run();
// a
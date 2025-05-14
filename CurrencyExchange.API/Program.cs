using CurrencyExchange.API.Helpers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();

builder.Services.AddControllers();

builder.AddApiVersioning();

builder.Services.AddEndpointsApiExplorer();

builder.AddSwagger();

builder.AddJwtAuth();

builder.Services.AddOpenApi();

builder.Logging.ClearProviders();

builder.AddSerilog();

builder.AddOpenTelemetryTracing();

builder.Services.AddCurrencyServices();

var app = builder.Build();

app.UseExceptionMiddleware();

app.UseLoggingMiddleware();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
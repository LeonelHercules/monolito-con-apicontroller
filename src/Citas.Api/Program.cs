using System.Text.Json.Serialization;
using Citas.Api.Errores;
using Citas.Application;
using Citas.Infrastructure;
using Citas.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Composición: cada capa registra lo suyo
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// 2. Controladores
builder.Services
    .AddControllers(options =>
    {
        // Sin esto, MVC trata cada string no-nullable como [Required] y responde 400
        // antes de llegar a FluentValidation. Así, la validación vive en un solo lugar.
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    })
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// 3. Errores y OpenAPI
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();               // /openapi/v1.json
    app.MapScalarApiReference();    // /scalar

    // Aplica las migraciones pendientes al arrancar (solo en desarrollo)
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.MapControllers();

app.Run();

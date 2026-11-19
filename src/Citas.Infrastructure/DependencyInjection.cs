using Citas.Application.Abstractions;
using Citas.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Citas.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Falta la cadena de conexión 'Default'.");

        services.AddDbContext<AppDbContext>(options => options
            .UseSqlServer(connectionString, sql => sql
                .MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)
                // Reintenta solo ante errores transitorios de red o de SQL Server
                .EnableRetryOnFailure()));

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        return services;
    }
}

using Microsoft.EntityFrameworkCore;

namespace Combinado.Infrastructure.Persistence;

/// <summary>
/// Configuração única do provider Npgsql, compartilhada entre runtime e design-time
/// para que migrations e app vejam exatamente o mesmo modelo.
/// </summary>
public static class PostgresOptions
{
    public static void Configure(DbContextOptionsBuilder options, string connectionString)
    {
        options
            .UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsAssembly(typeof(CombinadoDbContext).Assembly.GetName().Name);
                npgsql.EnableRetryOnFailure(maxRetryCount: 3);
            })
            .UseSnakeCaseNamingConvention();
    }
}

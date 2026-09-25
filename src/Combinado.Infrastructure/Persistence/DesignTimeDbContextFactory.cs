using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Combinado.Infrastructure.Persistence;

/// <summary>
/// Permite <c>dotnet ef</c> funcionar sem um startup project rodando a Api inteira.
/// Lê <c>ConnectionStrings__Postgres</c> do ambiente; cai no compose local se ausente.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<CombinadoDbContext>
{
    private const string LocalComposeConnectionString =
        "Host=localhost;Port=5435;Database=combinado;Username=combinado;Password=combinado";

    public CombinadoDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")
            ?? LocalComposeConnectionString;

        var options = new DbContextOptionsBuilder<CombinadoDbContext>();
        PostgresOptions.Configure(options, connectionString);

        return new CombinadoDbContext(options.Options);
    }
}

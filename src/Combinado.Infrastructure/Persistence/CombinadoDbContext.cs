using Microsoft.EntityFrameworkCore;

namespace Combinado.Infrastructure.Persistence;

/// <summary>
/// DbContext único do monolito. Aggregates são adicionados por fase via <c>IEntityTypeConfiguration</c>
/// em <c>Persistence/Configurations/</c>, descobertas automaticamente por assembly.
/// </summary>
public sealed class CombinadoDbContext(DbContextOptions<CombinadoDbContext> options) : DbContext(options)
{
    public const string DefaultSchema = "public";

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(DefaultSchema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CombinadoDbContext).Assembly);
    }
}

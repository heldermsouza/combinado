using Combinado.Application.Common;

namespace Combinado.Infrastructure.Persistence;

/// <summary>
/// Commit via EF Core. O despacho de domain events (após o commit) entra junto com o MediatR no PR de auth.
/// </summary>
public sealed class UnitOfWork(CombinadoDbContext db) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}

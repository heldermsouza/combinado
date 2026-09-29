namespace Combinado.Application.Common;

/// <summary>
/// Commit da transação da requisição. Handlers mudam agregados via repositórios e chamam
/// <see cref="SaveChangesAsync"/> uma vez; domain events são despachados após o commit.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

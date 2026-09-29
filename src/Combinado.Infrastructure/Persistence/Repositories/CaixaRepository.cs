using Combinado.Application.Caixas;
using Combinado.Domain.Caixas;
using Combinado.Domain.Common;
using Combinado.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Combinado.Infrastructure.Persistence.Repositories;

public sealed class CaixaRepository(CombinadoDbContext db) : ICaixaRepository
{
    public Task<Caixa?> GetByIdAsync(CaixaId id, CancellationToken cancellationToken) =>
        db.Caixas.SingleOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<Caixa?> GetByNumeroWhatsAppAsync(NumeroWhatsApp numero, CancellationToken cancellationToken) =>
        db.Caixas.SingleOrDefaultAsync(c => c.Parceiros.Any(p => p.NumeroWhatsApp == numero), cancellationToken);

    public Task<bool> NumeroJaVinculadoAsync(NumeroWhatsApp numero, CancellationToken cancellationToken) =>
        db.Set<Parceiro>().AnyAsync(p => p.NumeroWhatsApp == numero, cancellationToken);

    public void Add(Caixa caixa) => db.Caixas.Add(caixa);
}

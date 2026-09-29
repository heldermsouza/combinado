using Combinado.Domain.Caixas;
using Combinado.Domain.Common;
using Combinado.Domain.ValueObjects;

namespace Combinado.Application.Caixas;

public interface ICaixaRepository
{
    Task<Caixa?> GetByIdAsync(CaixaId id, CancellationToken cancellationToken);

    /// <summary>Caixa que contém o parceiro (fundador ou convidado) com este número.</summary>
    Task<Caixa?> GetByNumeroWhatsAppAsync(NumeroWhatsApp numero, CancellationToken cancellationToken);

    /// <summary>RN01 entre Caixas: um número só pode existir em um Caixa.</summary>
    Task<bool> NumeroJaVinculadoAsync(NumeroWhatsApp numero, CancellationToken cancellationToken);

    void Add(Caixa caixa);
}

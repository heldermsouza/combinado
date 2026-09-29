using Combinado.Domain.Common;

namespace Combinado.Domain.Caixas.Events;

/// <summary>Parceiro respondeu "SIM COMBINADO". Caixa passou a Ativo; avisar os dois.</summary>
public sealed record ParceiroVinculado(
    CaixaId CaixaId,
    ParceiroId ParceiroId,
    DateTimeOffset OcorridoEm) : IDomainEvent;

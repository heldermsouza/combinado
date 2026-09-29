using Combinado.Domain.Common;
using Combinado.Domain.ValueObjects;

namespace Combinado.Domain.Caixas.Events;

/// <summary>Fundador convidou o parceiro. Dispara a mensagem de convite no WhatsApp.</summary>
public sealed record ParceiroConvidado(
    CaixaId CaixaId,
    ParceiroId ParceiroId,
    string NomeConvidado,
    NumeroWhatsApp NumeroConvidado,
    string NomeFundador,
    DateTimeOffset OcorridoEm) : IDomainEvent;

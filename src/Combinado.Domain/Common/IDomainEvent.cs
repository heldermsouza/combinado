namespace Combinado.Domain.Common;

/// <summary>
/// Fato relevante do domínio que dispara side effects fora do aggregate (notificação, alerta, e-mail).
/// Despachado após o commit da unidade de trabalho.
/// </summary>
public interface IDomainEvent
{
    DateTimeOffset OcorridoEm { get; }
}

namespace Combinado.Domain.Tests.Support;

/// <summary>Relógio determinístico para testes. Avance com <see cref="Avancar"/>.</summary>
public sealed class FixedTimeProvider(DateTimeOffset agora) : TimeProvider
{
    public static readonly DateTimeOffset Padrao = new(2026, 9, 25, 15, 0, 0, TimeSpan.Zero);

    private DateTimeOffset _agora = agora;

    public FixedTimeProvider()
        : this(Padrao)
    {
    }

    public override DateTimeOffset GetUtcNow() => _agora;

    public void Avancar(TimeSpan delta) => _agora = _agora.Add(delta);
}

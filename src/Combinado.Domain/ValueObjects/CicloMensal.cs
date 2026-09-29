using System.Globalization;
using Combinado.Domain.Common;

namespace Combinado.Domain.ValueObjects;

/// <summary>
/// Mês de orçamento (<c>YYYY-MM</c>). RN04: o ciclo vira no dia 1 às 00:00 no timezone do casal,
/// por isso início e fim são calculados a partir do timezone, nunca de UTC direto.
/// </summary>
public readonly record struct CicloMensal : IComparable<CicloMensal>
{
    public CicloMensal(int ano, int mes)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(ano, 2000);
        ArgumentOutOfRangeException.ThrowIfLessThan(mes, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(mes, 12);
        Ano = ano;
        Mes = mes;
    }

    public int Ano { get; }

    public int Mes { get; }

    /// <summary>Ciclo que contém <paramref name="instante"/> no timezone informado.</summary>
    public static CicloMensal De(DateTimeOffset instante, TimeZoneInfo timezone)
    {
        var local = TimeZoneInfo.ConvertTime(instante, timezone);
        return new CicloMensal(local.Year, local.Month);
    }

    public static CicloMensal Atual(TimeProvider clock, TimeZoneInfo timezone) => De(clock.GetUtcNow(), timezone);

    public static Result<CicloMensal> Parse(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto)
            || !DateTime.TryParseExact(texto, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var data))
        {
            return Error.Validation("ciclo_mensal.invalido", "Ciclo deve estar no formato YYYY-MM.");
        }

        return new CicloMensal(data.Year, data.Month);
    }

    /// <summary>Primeiro instante do ciclo (dia 1, 00:00) no timezone.</summary>
    public DateTimeOffset Inicio(TimeZoneInfo timezone)
    {
        var primeiroDia = new DateTime(Ano, Mes, 1, 0, 0, 0, DateTimeKind.Unspecified);
        return new DateTimeOffset(primeiroDia, timezone.GetUtcOffset(primeiroDia));
    }

    /// <summary>Primeiro instante do ciclo seguinte; o ciclo é o intervalo [Inicio, Fim).</summary>
    public DateTimeOffset Fim(TimeZoneInfo timezone) => Proximo().Inicio(timezone);

    public bool Contem(DateTimeOffset instante, TimeZoneInfo timezone) => De(instante, timezone) == this;

    public CicloMensal Proximo() => Mes == 12 ? new CicloMensal(Ano + 1, 1) : new CicloMensal(Ano, Mes + 1);

    public CicloMensal Anterior() => Mes == 1 ? new CicloMensal(Ano - 1, 12) : new CicloMensal(Ano, Mes - 1);

    public int CompareTo(CicloMensal other) => (Ano * 100 + Mes).CompareTo(other.Ano * 100 + other.Mes);

    public static bool operator <(CicloMensal a, CicloMensal b) => a.CompareTo(b) < 0;

    public static bool operator >(CicloMensal a, CicloMensal b) => a.CompareTo(b) > 0;

    public static bool operator <=(CicloMensal a, CicloMensal b) => a.CompareTo(b) <= 0;

    public static bool operator >=(CicloMensal a, CicloMensal b) => a.CompareTo(b) >= 0;

    public override string ToString() => $"{Ano:D4}-{Mes:D2}";
}

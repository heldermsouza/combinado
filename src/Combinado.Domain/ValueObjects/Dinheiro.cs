using System.Globalization;
using Combinado.Domain.Common;

namespace Combinado.Domain.ValueObjects;

/// <summary>
/// Valor monetário com moeda. Sempre 2 casas decimais (arredondamento bancário).
/// Operações entre moedas diferentes são erro de programação e lançam.
/// </summary>
public readonly record struct Dinheiro : IComparable<Dinheiro>
{
    public const string MoedaPadrao = "BRL";

    private static readonly CultureInfo CulturaBr = CultureInfo.GetCultureInfo("pt-BR");

    private Dinheiro(decimal valor, string moeda)
    {
        Valor = decimal.Round(valor, 2, MidpointRounding.ToEven);
        Moeda = moeda;
    }

    public decimal Valor { get; }

    public string Moeda { get; }

    public static Dinheiro Zero => new(0m, MoedaPadrao);

    public bool EhZero => Valor == 0m;

    /// <summary>Uso interno e testes: valor conhecido, não negativo.</summary>
    public static Dinheiro Reais(decimal valor)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(valor);
        return new Dinheiro(valor, MoedaPadrao);
    }

    /// <summary>Entrada de usuário: valida e devolve erro de domínio em vez de lançar.</summary>
    public static Result<Dinheiro> Criar(decimal valor, string moeda = MoedaPadrao)
    {
        if (valor < 0m)
        {
            return Error.Validation("dinheiro.negativo", "Valor não pode ser negativo.");
        }

        if (moeda != MoedaPadrao)
        {
            return Error.Validation("dinheiro.moeda_nao_suportada", $"Moeda {moeda} não suportada no MVP.");
        }

        return new Dinheiro(valor, moeda);
    }

    public static Dinheiro operator +(Dinheiro a, Dinheiro b)
    {
        GarantirMesmaMoeda(a, b);
        return new Dinheiro(a.Valor + b.Valor, a.Moeda);
    }

    public static Dinheiro operator -(Dinheiro a, Dinheiro b)
    {
        GarantirMesmaMoeda(a, b);
        return new Dinheiro(a.Valor - b.Valor, a.Moeda);
    }

    public static Dinheiro operator *(Dinheiro a, decimal fator) => new(a.Valor * fator, a.Moeda);

    public static bool operator <(Dinheiro a, Dinheiro b) => a.CompareTo(b) < 0;

    public static bool operator >(Dinheiro a, Dinheiro b) => a.CompareTo(b) > 0;

    public static bool operator <=(Dinheiro a, Dinheiro b) => a.CompareTo(b) <= 0;

    public static bool operator >=(Dinheiro a, Dinheiro b) => a.CompareTo(b) >= 0;

    public static Dinheiro Add(Dinheiro a, Dinheiro b) => a + b;

    public static Dinheiro Subtract(Dinheiro a, Dinheiro b) => a - b;

    public static Dinheiro Multiply(Dinheiro a, decimal fator) => a * fator;

    public int CompareTo(Dinheiro other)
    {
        GarantirMesmaMoeda(this, other);
        return Valor.CompareTo(other.Valor);
    }

    /// <summary>Percentual de <paramref name="total"/> que este valor representa (0 se total for zero).</summary>
    public decimal PercentualDe(Dinheiro total)
    {
        GarantirMesmaMoeda(this, total);
        return total.EhZero ? 0m : decimal.Round(Valor / total.Valor, 4, MidpointRounding.ToEven);
    }

    public override string ToString() => Valor.ToString("C2", CulturaBr);

    private static void GarantirMesmaMoeda(Dinheiro a, Dinheiro b)
    {
        if (a.Moeda != b.Moeda)
        {
            throw new InvalidOperationException($"Operação entre moedas diferentes: {a.Moeda} e {b.Moeda}.");
        }
    }
}

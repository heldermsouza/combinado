using Combinado.Domain.ValueObjects;

namespace Combinado.Domain.Tests.ValueObjects;

public sealed class DinheiroTests
{
    [Fact]
    public void Criar_arredonda_para_duas_casas_bancario()
    {
        Assert.Equal(10.12m, Dinheiro.Criar(10.125m).Value.Valor);   // ToEven: 10.125 → 10.12
        Assert.Equal(10.14m, Dinheiro.Criar(10.135m).Value.Valor);   // ToEven: 10.135 → 10.14
    }

    [Fact]
    public void Criar_rejeita_negativo_e_moeda_nao_suportada()
    {
        Assert.Equal("dinheiro.negativo", Dinheiro.Criar(-1m).Error.Code);
        Assert.Equal("dinheiro.moeda_nao_suportada", Dinheiro.Criar(1m, "USD").Error.Code);
    }

    [Fact]
    public void Reais_lanca_para_negativo()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Dinheiro.Reais(-0.01m));
    }

    [Fact]
    public void Soma_subtrai_e_compara()
    {
        var a = Dinheiro.Reais(100m);
        var b = Dinheiro.Reais(30.5m);

        Assert.Equal(Dinheiro.Reais(130.5m), a + b);
        Assert.Equal(Dinheiro.Reais(69.5m), a - b);
        Assert.True(a > b);
        Assert.True(b <= a);
        Assert.Equal(Dinheiro.Reais(61m), b * 2);
    }

    [Fact]
    public void Percentual_de_total()
    {
        Assert.Equal(0.8m, Dinheiro.Reais(800m).PercentualDe(Dinheiro.Reais(1000m)));
        Assert.Equal(1.2m, Dinheiro.Reais(1200m).PercentualDe(Dinheiro.Reais(1000m)));
        Assert.Equal(0m, Dinheiro.Reais(50m).PercentualDe(Dinheiro.Zero));
    }

    [Fact]
    public void ToString_em_pt_BR()
    {
        var texto = Dinheiro.Reais(1234.5m).ToString();

        Assert.Contains("1.234,50", texto, StringComparison.Ordinal);
        Assert.Contains("R$", texto, StringComparison.Ordinal);
    }
}

using Combinado.Domain.Caixas;
using Combinado.Domain.Tests.Support;

namespace Combinado.Domain.Tests.Caixas;

/// <summary>
/// RN01 — Vínculo: número WA pertence a exatamente 1 Caixa; rejeita duplicado.
/// A parte "entre Caixas" é coberta pelo índice único em CaixaRepositoryTests (integração).
/// </summary>
public sealed class RN01_VinculoTests
{
    private readonly FixedTimeProvider _clock = new();

    [Fact]
    public void RN01_numero_do_fundador_nao_pode_ser_convidado_no_mesmo_caixa()
    {
        var caixa = Caixa.Criar("Ana", Numeros.Ana, _clock).Value;

        var result = caixa.ConvidarParceiro("Ana de novo", Numeros.Ana, _clock);

        Assert.Equal("caixa.numero_do_fundador", result.Error.Code);
        Assert.Single(caixa.Parceiros);
    }

    [Fact]
    public void RN01_caixa_nao_aceita_terceiro_parceiro()
    {
        var caixa = Caixa.Criar("Ana", Numeros.Ana, _clock).Value;
        caixa.ConvidarParceiro("Bruno", Numeros.Bruno, _clock);

        var result = caixa.ConvidarParceiro("Carla", Numeros.Carla, _clock);

        Assert.Equal("caixa.convite_pendente", result.Error.Code);
        Assert.Equal(Caixa.MaxParceiros, caixa.Parceiros.Count);
    }

    [Fact]
    public void RN01_caixa_ativo_nao_aceita_novo_convite()
    {
        var caixa = Caixa.Criar("Ana", Numeros.Ana, _clock).Value;
        caixa.ConvidarParceiro("Bruno", Numeros.Bruno, _clock);
        caixa.VincularParceiro(Numeros.Bruno, _clock);

        var result = caixa.ConvidarParceiro("Carla", Numeros.Carla, _clock);

        Assert.Equal("caixa.ja_ativo", result.Error.Code);
    }

    [Fact]
    public void RN01_PossuiNumero_reconhece_fundador_e_convidado()
    {
        var caixa = Caixa.Criar("Ana", Numeros.Ana, _clock).Value;
        caixa.ConvidarParceiro("Bruno", Numeros.Bruno, _clock);

        Assert.True(caixa.PossuiNumero(Numeros.Ana));
        Assert.True(caixa.PossuiNumero(Numeros.Bruno));
        Assert.False(caixa.PossuiNumero(Numeros.Carla));
    }
}

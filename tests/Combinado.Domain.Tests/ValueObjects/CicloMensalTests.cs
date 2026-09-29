using Combinado.Domain.Tests.Support;
using Combinado.Domain.ValueObjects;

namespace Combinado.Domain.Tests.ValueObjects;

public sealed class CicloMensalTests
{
    private static readonly TimeZoneInfo SaoPaulo = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    [Fact]
    public void RN04_virada_do_ciclo_segue_o_timezone_do_casal_nao_UTC()
    {
        // 01/10 às 02:30 UTC ainda é 30/09 às 23:30 em São Paulo (UTC-3): o ciclo é setembro.
        var instante = new DateTimeOffset(2026, 10, 1, 2, 30, 0, TimeSpan.Zero);

        var ciclo = CicloMensal.De(instante, SaoPaulo);

        Assert.Equal(new CicloMensal(2026, 9), ciclo);
    }

    [Fact]
    public void RN04_inicio_e_dia_1_meia_noite_local()
    {
        var ciclo = new CicloMensal(2026, 10);

        var inicio = ciclo.Inicio(SaoPaulo);

        Assert.Equal(new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.FromHours(-3)), inicio);
        Assert.Equal(new DateTimeOffset(2026, 10, 1, 3, 0, 0, TimeSpan.Zero), inicio.ToUniversalTime());
    }

    [Fact]
    public void Intervalo_e_fechado_no_inicio_e_aberto_no_fim()
    {
        var ciclo = new CicloMensal(2026, 9);

        Assert.True(ciclo.Contem(ciclo.Inicio(SaoPaulo), SaoPaulo));
        Assert.False(ciclo.Contem(ciclo.Fim(SaoPaulo), SaoPaulo));
        Assert.True(ciclo.Contem(ciclo.Fim(SaoPaulo).AddTicks(-1), SaoPaulo));
    }

    [Fact]
    public void Proximo_e_anterior_atravessam_o_ano()
    {
        Assert.Equal(new CicloMensal(2027, 1), new CicloMensal(2026, 12).Proximo());
        Assert.Equal(new CicloMensal(2025, 12), new CicloMensal(2026, 1).Anterior());
    }

    [Fact]
    public void Parse_e_ToString_sao_inversos()
    {
        var ciclo = CicloMensal.Parse("2026-09").Value;

        Assert.Equal(new CicloMensal(2026, 9), ciclo);
        Assert.Equal("2026-09", ciclo.ToString());
        Assert.Equal("ciclo_mensal.invalido", CicloMensal.Parse("09/2026").Error.Code);
    }

    [Fact]
    public void Atual_usa_o_relogio_injetado()
    {
        var clock = new FixedTimeProvider(new DateTimeOffset(2026, 3, 1, 1, 0, 0, TimeSpan.Zero)); // 28/02 22:00 em SP

        Assert.Equal(new CicloMensal(2026, 2), CicloMensal.Atual(clock, SaoPaulo));
    }

    [Fact]
    public void Comparacao_cronologica()
    {
        Assert.True(new CicloMensal(2026, 9) < new CicloMensal(2026, 10));
        Assert.True(new CicloMensal(2027, 1) > new CicloMensal(2026, 12));
    }
}

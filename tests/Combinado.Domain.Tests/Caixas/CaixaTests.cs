using Combinado.Domain.Caixas;
using Combinado.Domain.Caixas.Events;
using Combinado.Domain.Tests.Support;

namespace Combinado.Domain.Tests.Caixas;

public sealed class CaixaTests
{
    private readonly FixedTimeProvider _clock = new();

    [Fact]
    public void Criar_nasce_pendente_com_fundador_vinculado_e_categorias_padrao()
    {
        var caixa = Caixa.Criar("Ana", Numeros.Ana, _clock).Value;

        Assert.Equal(StatusCaixa.Pendente, caixa.Status);
        Assert.Equal(Caixa.TimezonePadrao, caixa.Timezone);
        Assert.Single(caixa.Parceiros);
        Assert.True(caixa.Fundador.Vinculado);
        Assert.True(caixa.Fundador.EmailPrimario);
        Assert.Equal(Categoria.Padroes.Count, caixa.Categorias.Count);
        Assert.All(caixa.Categorias, c => Assert.True(c.Padrao));
        Assert.Equal(Categoria.Padroes.Select(p => p.Slug), caixa.Categorias.OrderBy(c => c.Ordem).Select(c => c.Slug));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_rejeita_nome_vazio(string nome)
    {
        Assert.Equal("parceiro.nome_vazio", Caixa.Criar(nome, Numeros.Ana, _clock).Error.Code);
    }

    [Fact]
    public void Criar_rejeita_nome_longo_e_apara_espacos()
    {
        Assert.Equal("parceiro.nome_longo", Caixa.Criar(new string('a', 81), Numeros.Ana, _clock).Error.Code);
        Assert.Equal("Ana", Caixa.Criar("  Ana  ", Numeros.Ana, _clock).Value.Fundador.Nome);
    }

    [Fact]
    public void ConvidarParceiro_adiciona_convidado_nao_vinculado_e_emite_evento()
    {
        var caixa = Caixa.Criar("Ana", Numeros.Ana, _clock).Value;

        var convidado = caixa.ConvidarParceiro("Bruno", Numeros.Bruno, _clock).Value;

        Assert.Equal(2, caixa.Parceiros.Count);
        Assert.False(convidado.Vinculado);
        Assert.False(convidado.EmailPrimario);
        Assert.Equal(StatusCaixa.Pendente, caixa.Status);
        var evento = Assert.Single(caixa.DomainEvents.OfType<ParceiroConvidado>());
        Assert.Equal(Numeros.Bruno, evento.NumeroConvidado);
        Assert.Equal("Ana", evento.NomeFundador);
    }

    [Fact]
    public void VincularParceiro_ativa_o_caixa_e_emite_evento()
    {
        var caixa = Caixa.Criar("Ana", Numeros.Ana, _clock).Value;
        caixa.ConvidarParceiro("Bruno", Numeros.Bruno, _clock);
        _clock.Avancar(TimeSpan.FromMinutes(5));

        var result = caixa.VincularParceiro(Numeros.Bruno, _clock);

        Assert.True(result.IsSuccess);
        Assert.Equal(StatusCaixa.Ativo, caixa.Status);
        Assert.Equal(_clock.GetUtcNow(), caixa.Convidado!.VinculadoEm);
        Assert.Single(caixa.DomainEvents.OfType<ParceiroVinculado>());
    }

    [Fact]
    public void VincularParceiro_sem_convite_para_o_numero_falha()
    {
        var caixa = Caixa.Criar("Ana", Numeros.Ana, _clock).Value;
        caixa.ConvidarParceiro("Bruno", Numeros.Bruno, _clock);

        var result = caixa.VincularParceiro(Numeros.Carla, _clock);

        Assert.Equal("caixa.convite_nao_encontrado", result.Error.Code);
        Assert.Equal(StatusCaixa.Pendente, caixa.Status);
    }

    [Fact]
    public void VincularParceiro_duas_vezes_falha_na_segunda()
    {
        var caixa = Caixa.Criar("Ana", Numeros.Ana, _clock).Value;
        caixa.ConvidarParceiro("Bruno", Numeros.Bruno, _clock);
        caixa.VincularParceiro(Numeros.Bruno, _clock);

        var result = caixa.VincularParceiro(Numeros.Bruno, _clock);

        Assert.Equal("caixa.parceiro_ja_vinculado", result.Error.Code);
    }

    [Fact]
    public void Fundador_nao_pode_se_vincular_como_convidado()
    {
        var caixa = Caixa.Criar("Ana", Numeros.Ana, _clock).Value;

        var result = caixa.VincularParceiro(Numeros.Ana, _clock);

        Assert.Equal("caixa.convite_nao_encontrado", result.Error.Code);
    }
}

using Combinado.Domain.ValueObjects;

namespace Combinado.Domain.Tests.ValueObjects;

public sealed class NumeroWhatsAppTests
{
    [Theory]
    [InlineData("+55 11 91234-5678", "+5511912345678")]
    [InlineData("(11) 91234-5678", "+5511912345678")]
    [InlineData("11912345678", "+5511912345678")]
    [InlineData("5511912345678", "+5511912345678")]
    [InlineData("011912345678", "+5511912345678")]
    [InlineData("+55 (21) 9 8765-4321", "+5521987654321")]
    public void Normaliza_para_E164(string entrada, string esperado)
    {
        var result = NumeroWhatsApp.Criar(entrada);

        Assert.True(result.IsSuccess);
        Assert.Equal(esperado, result.Value.Valor);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("1191234567")]        // 8 dígitos (fixo ou celular antigo)
    [InlineData("11 3123-4567")]      // fixo
    [InlineData("01912345678")]       // DDD com zero
    [InlineData("+1 415 555 2671")]   // não é Brasil
    [InlineData("abc")]
    [InlineData("119123456789")]      // dígito a mais
    public void Rejeita_entrada_invalida(string entrada)
    {
        var result = NumeroWhatsApp.Criar(entrada);

        Assert.True(result.IsFailure);
        Assert.StartsWith("numero_whatsapp.", result.Error.Code, StringComparison.Ordinal);
    }

    [Fact]
    public void Nulo_retorna_erro_de_vazio()
    {
        var result = NumeroWhatsApp.Criar(null);

        Assert.Equal("numero_whatsapp.vazio", result.Error.Code);
    }

    [Fact]
    public void Expoe_ddd_local_e_formatado()
    {
        var numero = NumeroWhatsApp.Criar("11912345678").Value;

        Assert.Equal("11", numero.Ddd);
        Assert.Equal("912345678", numero.NumeroLocal);
        Assert.Equal("+55 (11) 91234-5678", numero.Formatado);
    }

    [Fact]
    public void Igualdade_por_valor()
    {
        var a = NumeroWhatsApp.Criar("(11) 91234-5678").Value;
        var b = NumeroWhatsApp.Criar("+5511912345678").Value;

        Assert.Equal(a, b);
        Assert.True(a == b);
    }

    [Fact]
    public void DeE164_lanca_se_persistencia_estiver_corrompida()
    {
        Assert.Throws<ArgumentException>(() => NumeroWhatsApp.DeE164("lixo"));
    }
}

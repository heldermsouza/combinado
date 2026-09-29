using Combinado.Domain.Common;

namespace Combinado.Domain.Tests.Common;

public sealed class ResultTests
{
    private static readonly Error SampleError = Error.Validation("teste.invalido", "Valor inválido");

    [Fact]
    public void Success_sem_valor_nao_carrega_erro()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(Error.None, result.Error);
    }

    [Fact]
    public void Failure_expoe_erro_e_tipo()
    {
        var result = Result.Failure(SampleError);

        Assert.True(result.IsFailure);
        Assert.Equal("teste.invalido", result.Error.Code);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public void Success_com_valor_expoe_valor()
    {
        var result = Result.Success(42);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Value_em_falha_lanca_InvalidOperation()
    {
        var result = Result.Failure<int>(SampleError);

        var ex = Assert.Throws<InvalidOperationException>(() => result.Value);
        Assert.Contains("teste.invalido", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Conversao_implicita_de_valor_e_de_erro()
    {
        Result<string> ok = "abc";
        Result<string> fail = SampleError;

        Assert.True(ok.IsSuccess);
        Assert.Equal("abc", ok.Value);
        Assert.True(fail.IsFailure);
        Assert.Equal(SampleError, fail.Error);
    }

    [Fact]
    public void Map_transforma_valor_em_sucesso_e_propaga_erro_em_falha()
    {
        var ok = Result.Success(2).Map(x => x * 10);
        var fail = Result.Failure<int>(SampleError).Map(x => x * 10);

        Assert.Equal(20, ok.Value);
        Assert.True(fail.IsFailure);
        Assert.Equal(SampleError, fail.Error);
    }

    [Fact]
    public void Bind_encadeia_e_curto_circuita_na_primeira_falha()
    {
        static Result<int> Dobra(int x) => x * 2;
        static Result<int> Falha(int _) => SampleError;

        var ok = Result.Success(3).Bind(Dobra).Bind(Dobra);
        var fail = Result.Success(3).Bind(Falha).Bind(Dobra);

        Assert.Equal(12, ok.Value);
        Assert.True(fail.IsFailure);
        Assert.Equal(SampleError, fail.Error);
    }

    [Fact]
    public void Match_escolhe_ramo_certo()
    {
        var ok = Result.Success(1).Match(v => $"ok:{v}", e => $"erro:{e.Code}");
        var fail = Result.Failure<int>(SampleError).Match(v => $"ok:{v}", e => $"erro:{e.Code}");

        Assert.Equal("ok:1", ok);
        Assert.Equal("erro:teste.invalido", fail);
    }

    [Fact]
    public void Failure_generico_a_partir_de_Result_nao_generico_preserva_erro()
    {
        Result falhou = Result.Failure(SampleError);

        var propagado = Result.Failure<decimal>(falhou);

        Assert.True(propagado.IsFailure);
        Assert.Equal(SampleError, propagado.Error);
    }

    [Fact]
    public void Failure_generico_a_partir_de_sucesso_e_erro_de_programacao()
    {
        var sucesso = Result.Success();

        Assert.Throws<InvalidOperationException>(() => Result.Failure<int>(sucesso));
    }
}

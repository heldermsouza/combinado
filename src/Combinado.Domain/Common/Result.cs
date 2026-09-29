using System.Diagnostics.CodeAnalysis;

namespace Combinado.Domain.Common;

/// <summary>
/// Resultado de uma operação sem valor de retorno. Substitui exceções para fluxo de controle (CLAUDE.md §4).
/// </summary>
public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
        {
            throw new ArgumentException("Resultado de sucesso não pode carregar erro.", nameof(error));
        }

        if (!isSuccess && error == Error.None)
        {
            throw new ArgumentException("Resultado de falha precisa de um erro.", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error Error { get; }

    public static Result Success() => new(true, Error.None);

    public static Result Failure(Error error) => new(false, error);

    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);

    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);

    /// <summary>Converte uma falha para outro tipo de valor sem tocar no erro.</summary>
    public static Result<TValue> Failure<TValue>(Result failed)
    {
        if (failed.IsSuccess)
        {
            throw new InvalidOperationException("Só um resultado de falha pode ser propagado como falha.");
        }

        return Failure<TValue>(failed.Error);
    }
}

/// <summary>
/// Resultado de uma operação com valor. <see cref="Value"/> só é acessível em sucesso.
/// </summary>
public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    [NotNull]
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException($"Resultado de falha não tem valor. Erro: {Error.Code}");

    public static implicit operator Result<TValue>(TValue value) => Success(value);

    public static implicit operator Result<TValue>(Error error) => Failure<TValue>(error);

    public Result<TOut> Map<TOut>(Func<TValue, TOut> map) =>
        IsSuccess ? Success(map(Value)) : Failure<TOut>(Error);

    public Result<TOut> Bind<TOut>(Func<TValue, Result<TOut>> bind) =>
        IsSuccess ? bind(Value) : Failure<TOut>(Error);

    public TOut Match<TOut>(Func<TValue, TOut> onSuccess, Func<Error, TOut> onFailure) =>
        IsSuccess ? onSuccess(Value) : onFailure(Error);
}

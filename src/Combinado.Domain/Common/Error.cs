namespace Combinado.Domain.Common;

/// <summary>
/// Categoria do erro. A camada HTTP mapeia para status code; o domínio só escolhe a categoria.
/// </summary>
public enum ErrorType
{
    Failure,
    Validation,
    NotFound,
    Conflict,
    Forbidden,
}

/// <summary>
/// Erro de domínio/aplicação. <see cref="Code"/> é estável e legível por máquina
/// (ex.: <c>caixa.numero_ja_vinculado</c>); <see cref="Message"/> é para humanos, em PT-BR.
/// </summary>
public sealed record Error(string Code, string Message, ErrorType Type = ErrorType.Failure)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    public static Error Validation(string code, string message) => new(code, message, ErrorType.Validation);

    public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);

    public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);

    public static Error Forbidden(string code, string message) => new(code, message, ErrorType.Forbidden);
}

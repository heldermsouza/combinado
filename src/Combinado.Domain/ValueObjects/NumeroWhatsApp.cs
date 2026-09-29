using System.Text.RegularExpressions;
using Combinado.Domain.Common;

namespace Combinado.Domain.ValueObjects;

/// <summary>
/// Número de WhatsApp brasileiro em E.164 (<c>+55DDNNNNNNNNN</c>). Aceita entrada com máscara,
/// espaços, DDI opcional e zero à esquerda do DDD; rejeita fixo (só celular com 9 dígitos, iniciando em 9).
/// </summary>
public sealed partial record NumeroWhatsApp
{
    public const string DdiBrasil = "55";

    private NumeroWhatsApp(string e164)
    {
        Valor = e164;
    }

    /// <summary>Formato E.164, ex.: <c>+5511912345678</c>. É o que persiste e o que vai para o BSP.</summary>
    public string Valor { get; }

    public string Ddd => Valor.Substring(3, 2);

    public string NumeroLocal => Valor[5..];

    /// <summary>Ex.: <c>+55 (11) 91234-5678</c>.</summary>
    public string Formatado => $"+{DdiBrasil} ({Ddd}) {NumeroLocal[..5]}-{NumeroLocal[5..]}";

    public static Result<NumeroWhatsApp> Criar(string? entrada)
    {
        if (string.IsNullOrWhiteSpace(entrada))
        {
            return Error.Validation("numero_whatsapp.vazio", "Informe o número de WhatsApp.");
        }

        var digitos = NaoDigitos().Replace(entrada, string.Empty);

        if (digitos.StartsWith(DdiBrasil, StringComparison.Ordinal) && digitos.Length >= 12)
        {
            digitos = digitos[2..];
        }

        // "011 9..." — zero de operadora/tronco antes do DDD.
        if (digitos.Length == 12 && digitos[0] == '0')
        {
            digitos = digitos[1..];
        }

        if (!CelularBr().IsMatch(digitos))
        {
            return Error.Validation(
                "numero_whatsapp.invalido",
                "Número inválido. Use DDD + celular com 9 dígitos, ex.: (11) 91234-5678.");
        }

        return new NumeroWhatsApp($"+{DdiBrasil}{digitos}");
    }

    /// <summary>Reconstrói a partir de um E.164 já validado (persistência). Lança se inválido.</summary>
    public static NumeroWhatsApp DeE164(string e164)
    {
        var result = Criar(e164);
        return result.IsSuccess
            ? result.Value
            : throw new ArgumentException($"E.164 inválido na persistência: '{e164}'.", nameof(e164));
    }

    public override string ToString() => Valor;

    [GeneratedRegex(@"\D")]
    private static partial Regex NaoDigitos();

    // DDD: 11–99 sem zero inicial. Celular: 9 dígitos começando em 9.
    [GeneratedRegex(@"^[1-9][1-9]9\d{8}$")]
    private static partial Regex CelularBr();
}

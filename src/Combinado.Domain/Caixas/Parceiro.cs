using Combinado.Domain.Common;
using Combinado.Domain.ValueObjects;

namespace Combinado.Domain.Caixas;

/// <summary>
/// Uma das duas pessoas do casal. Criado pelo <see cref="Caixa"/> (fundador) ou por convite;
/// vira vinculado quando responde "SIM COMBINADO" no bot.
/// </summary>
public sealed class Parceiro : Entity<ParceiroId>
{
    public const int NomeMaxLength = 80;

    private Parceiro(ParceiroId id, string nome, NumeroWhatsApp numeroWhatsApp, bool emailPrimario, DateTimeOffset convidadoEm)
        : base(id)
    {
        Nome = nome;
        NumeroWhatsApp = numeroWhatsApp;
        EmailPrimario = emailPrimario;
        ConvidadoEm = convidadoEm;
    }

    public string Nome { get; private set; }

    public NumeroWhatsApp NumeroWhatsApp { get; private set; }

    /// <summary>Quem recebe os e-mails de conta/cobrança do casal. Exatamente um por Caixa.</summary>
    public bool EmailPrimario { get; private set; }

    public DateTimeOffset ConvidadoEm { get; }

    public DateTimeOffset? VinculadoEm { get; private set; }

    public bool Vinculado => VinculadoEm is not null;

    internal static Parceiro Fundador(string nome, NumeroWhatsApp numero, DateTimeOffset agora)
    {
        var parceiro = new Parceiro(ParceiroId.New(), nome, numero, emailPrimario: true, convidadoEm: agora);
        parceiro.VinculadoEm = agora;
        return parceiro;
    }

    internal static Parceiro Convidado(string nome, NumeroWhatsApp numero, DateTimeOffset agora) =>
        new(ParceiroId.New(), nome, numero, emailPrimario: false, convidadoEm: agora);

    internal void Vincular(DateTimeOffset agora) => VinculadoEm = agora;
}

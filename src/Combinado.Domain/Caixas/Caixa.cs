using Combinado.Domain.Caixas.Events;
using Combinado.Domain.Common;
using Combinado.Domain.ValueObjects;

namespace Combinado.Domain.Caixas;

/// <summary>
/// O "caixa compartilhado" do casal: aggregate root de parceiros e categorias.
/// Nasce <see cref="StatusCaixa.Pendente"/> com o fundador; vira <see cref="StatusCaixa.Ativo"/>
/// quando o segundo parceiro se vincula. Nunca tem mais de 2 parceiros (MVP).
/// </summary>
public sealed class Caixa : AggregateRoot<CaixaId>
{
    public const int MaxParceiros = 2;
    public const string TimezonePadrao = "America/Sao_Paulo";
    public const int TimezoneMaxLength = 64;

    private readonly List<Parceiro> _parceiros = [];
    private readonly List<Categoria> _categorias = [];

    private Caixa(CaixaId id, string timezone, DateTimeOffset criadoEm)
        : base(id)
    {
        Timezone = timezone;
        CriadoEm = criadoEm;
        Status = StatusCaixa.Pendente;
    }

    public StatusCaixa Status { get; private set; }

    /// <summary>IANA, ex.: <c>America/Sao_Paulo</c>. Define o dia 1 do ciclo (RN04).</summary>
    public string Timezone { get; private set; }

    public DateTimeOffset CriadoEm { get; }

    public IReadOnlyCollection<Parceiro> Parceiros => _parceiros.AsReadOnly();

    public IReadOnlyCollection<Categoria> Categorias => _categorias.AsReadOnly();

    public Parceiro Fundador => _parceiros.Single(p => p.EmailPrimario);

    public Parceiro? Convidado => _parceiros.SingleOrDefault(p => !p.EmailPrimario);

    public TimeZoneInfo TimeZoneInfo => TimeZoneInfo.FindSystemTimeZoneById(Timezone);

    /// <summary>Cria o Caixa com o fundador já vinculado e as categorias padrão.</summary>
    public static Result<Caixa> Criar(string nomeFundador, NumeroWhatsApp numeroFundador, TimeProvider clock)
    {
        var nome = ValidarNome(nomeFundador);
        if (nome.IsFailure)
        {
            return nome.Error;
        }

        var agora = clock.GetUtcNow();
        var caixa = new Caixa(CaixaId.New(), TimezonePadrao, agora);
        caixa._parceiros.Add(Parceiro.Fundador(nome.Value, numeroFundador, agora));

        var ordem = 0;
        foreach (var (nomeCategoria, slug) in Categoria.Padroes)
        {
            caixa._categorias.Add(Categoria.Nova(nomeCategoria, slug, ordem++, padrao: true));
        }

        return caixa;
    }

    /// <summary>
    /// Convida o segundo parceiro. RN01 dentro do agregado: número não pode repetir o do fundador.
    /// A unicidade entre Caixas é garantida pelo repositório + índice único.
    /// </summary>
    public Result<Parceiro> ConvidarParceiro(string nomeConvidado, NumeroWhatsApp numeroConvidado, TimeProvider clock)
    {
        if (Status == StatusCaixa.Ativo)
        {
            return Error.Conflict("caixa.ja_ativo", "Este casal já está completo.");
        }

        if (_parceiros.Count >= MaxParceiros)
        {
            return Error.Conflict("caixa.convite_pendente", "Já existe um convite pendente para este Caixa.");
        }

        if (_parceiros.Any(p => p.NumeroWhatsApp == numeroConvidado))
        {
            return Error.Validation("caixa.numero_do_fundador", "O número do parceiro não pode ser o seu próprio.");
        }

        var nome = ValidarNome(nomeConvidado);
        if (nome.IsFailure)
        {
            return nome.Error;
        }

        var agora = clock.GetUtcNow();
        var convidado = Parceiro.Convidado(nome.Value, numeroConvidado, agora);
        _parceiros.Add(convidado);

        RaiseDomainEvent(new ParceiroConvidado(Id, convidado.Id, convidado.Nome, convidado.NumeroWhatsApp, Fundador.Nome, agora));
        return convidado;
    }

    /// <summary>Parceiro respondeu "SIM COMBINADO" a partir do número convidado.</summary>
    public Result VincularParceiro(NumeroWhatsApp numero, TimeProvider clock)
    {
        var convidado = _parceiros.SingleOrDefault(p => p.NumeroWhatsApp == numero && !p.EmailPrimario);
        if (convidado is null)
        {
            return Error.NotFound("caixa.convite_nao_encontrado", "Não há convite pendente para este número.");
        }

        if (convidado.Vinculado)
        {
            return Error.Conflict("caixa.parceiro_ja_vinculado", "Este parceiro já está vinculado.");
        }

        var agora = clock.GetUtcNow();
        convidado.Vincular(agora);
        Status = StatusCaixa.Ativo;

        RaiseDomainEvent(new ParceiroVinculado(Id, convidado.Id, agora));
        return Result.Success();
    }

    public bool PossuiNumero(NumeroWhatsApp numero) => _parceiros.Any(p => p.NumeroWhatsApp == numero);

    private static Result<string> ValidarNome(string? nome)
    {
        var limpo = nome?.Trim();
        if (string.IsNullOrEmpty(limpo))
        {
            return Error.Validation("parceiro.nome_vazio", "Informe o nome.");
        }

        if (limpo.Length > Parceiro.NomeMaxLength)
        {
            return Error.Validation("parceiro.nome_longo", $"Nome deve ter até {Parceiro.NomeMaxLength} caracteres.");
        }

        return limpo;
    }
}

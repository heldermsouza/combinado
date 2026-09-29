using Combinado.Domain.Common;

namespace Combinado.Domain.Caixas;

/// <summary>
/// Categoria de gasto do casal. Cada Caixa nasce com o conjunto padrão e pode personalizar depois.
/// </summary>
public sealed class Categoria : Entity<CategoriaId>
{
    public const int NomeMaxLength = 40;
    public const int SlugMaxLength = 40;

    private Categoria(CategoriaId id, string nome, string slug, int ordem, bool padrao)
        : base(id)
    {
        Nome = nome;
        Slug = slug;
        Ordem = ordem;
        Padrao = padrao;
    }

    public string Nome { get; private set; }

    /// <summary>Identificador estável usado pelo prompt de categorização e pela UI (ex.: <c>mercado</c>).</summary>
    public string Slug { get; }

    public int Ordem { get; private set; }

    /// <summary>Veio do conjunto padrão do produto (não pode ser excluída, só renomeada).</summary>
    public bool Padrao { get; }

    internal static Categoria Nova(string nome, string slug, int ordem, bool padrao) =>
        new(CategoriaId.New(), nome, slug, ordem, padrao);

    /// <summary>
    /// Conjunto padrão do MVP. Ordem reflete a frequência esperada de uso pelo ICP.
    /// Revisar contra o PRD quando ele entrar no repo.
    /// </summary>
    public static IReadOnlyList<(string Nome, string Slug)> Padroes { get; } =
    [
        ("Moradia", "moradia"),
        ("Mercado", "mercado"),
        ("Alimentação fora", "alimentacao-fora"),
        ("Transporte", "transporte"),
        ("Saúde", "saude"),
        ("Lazer", "lazer"),
        ("Assinaturas", "assinaturas"),
        ("Pessoal", "pessoal"),
        ("Outros", "outros"),
    ];
}

namespace Combinado.Domain.Common;

// IDs fortes: evitam trocar CaixaId por ParceiroId sem o compilador reclamar.
// Guid v7 é ordenável por tempo, o que ajuda índices B-tree no Postgres.

public readonly record struct CaixaId(Guid Value)
{
    public static CaixaId New() => new(Guid.CreateVersion7());

    public override string ToString() => Value.ToString();
}

public readonly record struct ParceiroId(Guid Value)
{
    public static ParceiroId New() => new(Guid.CreateVersion7());

    public override string ToString() => Value.ToString();
}

public readonly record struct CategoriaId(Guid Value)
{
    public static CategoriaId New() => new(Guid.CreateVersion7());

    public override string ToString() => Value.ToString();
}

namespace Combinado.Domain.Common;

/// <summary>
/// Entidade com identidade forte. Igualdade por Id.
/// </summary>
public abstract class Entity<TId>
    where TId : struct, IEquatable<TId>
{
    protected Entity(TId id)
    {
        Id = id;
    }

    public TId Id { get; }

    public override bool Equals(object? obj) =>
        obj is Entity<TId> other && GetType() == other.GetType() && Id.Equals(other.Id);

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);
}

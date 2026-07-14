namespace FusionOS.SharedKernel;

/// <summary>
/// Base type for every domain entity in FusionOS. Per 04_DATABASE_GUIDELINES.md,
/// every entity is keyed by a UUID, never an auto-increment integer.
/// </summary>
public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();

    public override bool Equals(object? obj) =>
        obj is Entity other && other.GetType() == GetType() && other.Id == Id;

    public override int GetHashCode() => (GetType().ToString() + Id).GetHashCode();
}

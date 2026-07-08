namespace MiniGrc.Domain.Common;

/// <summary>
/// Base class for all domain entities. Carries the identity and audit timestamps
/// that every aggregate root and child entity shares.
/// </summary>
public abstract class Entity
{
    /// <summary>Surrogate primary key.</summary>
    public Guid Id { get; protected set; } = Guid.NewGuid();

    /// <summary>UTC instant the entity was first persisted.</summary>
    public DateTime CreatedAtUtc { get; protected set; } = DateTime.UtcNow;

    /// <summary>UTC instant of the last mutation; equals <see cref="CreatedAtUtc"/> until changed.</summary>
    public DateTime UpdatedAtUtc { get; protected set; } = DateTime.UtcNow;

    /// <summary>Mutates <see cref="UpdatedAtUtc"/>. Called by domain methods on every state change.</summary>
    protected void Touch() => UpdatedAtUtc = DateTime.UtcNow;
}

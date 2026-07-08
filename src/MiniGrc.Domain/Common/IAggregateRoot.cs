namespace MiniGrc.Domain.Common;

/// <summary>
/// Marker interface identifying an entity as an aggregate root. Aggregate roots are the
/// only entities that may be loaded or saved directly through a repository; child entities
/// are always reached through their root.
/// </summary>
public interface IAggregateRoot
{
}

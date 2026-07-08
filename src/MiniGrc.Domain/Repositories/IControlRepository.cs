using MiniGrc.Domain.Entities;
using MiniGrc.Domain.Enums;

namespace MiniGrc.Domain.Repositories;

/// <summary>
/// Read/write port for <see cref="Control"/> aggregates. Defined in the Domain layer so the
/// Application layer can depend on the abstraction without knowing about EF Core or PostgreSQL.
/// The concrete implementation lives in the Infrastructure layer.
/// </summary>
public interface IControlRepository
{
    /// <summary>Returns a control by id, or null when not found.</summary>
    Task<Control?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Returns all controls, optionally filtered by framework.</summary>
    Task<IReadOnlyList<Control>> GetAllAsync(ComplianceFramework? framework = null, CancellationToken ct = default);

    /// <summary>Finds a control by its unique code within a framework.</summary>
    Task<Control?> GetByCodeAsync(string code, CancellationToken ct = default);

    /// <summary>Adds a new control.</summary>
    Task AddAsync(Control control, CancellationToken ct = default);

    /// <summary>Persists changes to an existing tracked control.</summary>
    Task UpdateAsync(Control control, CancellationToken ct = default);

    /// <summary>Removes a control and its evidence.</summary>
    Task DeleteAsync(Control control, CancellationToken ct = default);
}

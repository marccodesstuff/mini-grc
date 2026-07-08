using MiniGrc.Domain.Entities;

namespace MiniGrc.Domain.Repositories;

/// <summary>
/// Read/write port for <see cref="Finding"/> aggregates, including their remediation tasks.
/// </summary>
public interface IFindingRepository
{
    /// <summary>Returns a finding by id with its remediation tasks loaded.</summary>
    Task<Finding?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Returns all findings, optionally only unmapped ones.</summary>
    Task<IReadOnlyList<Finding>> GetAllAsync(bool onlyUnmapped = false, CancellationToken ct = default);

    /// <summary>Returns findings whose <see cref="Finding.ExternalId"/> matches, for de-dup.</summary>
    Task<Finding?> GetByExternalIdAsync(string externalId, CancellationToken ct = default);

    /// <summary>Adds a new finding.</summary>
    Task AddAsync(Finding finding, CancellationToken ct = default);

    /// <summary>Persists changes to a tracked finding.</summary>
    Task UpdateAsync(Finding finding, CancellationToken ct = default);
}

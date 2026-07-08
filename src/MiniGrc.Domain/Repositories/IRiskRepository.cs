using MiniGrc.Domain.Entities;

namespace MiniGrc.Domain.Repositories;

/// <summary>Read/write port for <see cref="Risk"/> register entries.</summary>
public interface IRiskRepository
{
    /// <summary>Returns all risks.</summary>
    Task<IReadOnlyList<Risk>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Adds or updates a risk.</summary>
    Task UpsertAsync(Risk risk, CancellationToken ct = default);

    /// <summary>Removes a risk.</summary>
    Task DeleteAsync(Risk risk, CancellationToken ct = default);
}

using MiniGrc.Domain.Repositories;

namespace MiniGrc.Domain;

/// <summary>
/// Unit of Work port. Bundles the repository ports so a single CQRS handler can persist several
/// aggregates atomically. Implemented by the Infrastructure layer over EF Core's DbContext.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>Control repository.</summary>
    IControlRepository Controls { get; }

    /// <summary>Finding repository.</summary>
    IFindingRepository Findings { get; }

    /// <summary>Risk repository.</summary>
    IRiskRepository Risks { get; }

    /// <summary>Commits all pending changes atomically.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

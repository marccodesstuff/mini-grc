using Microsoft.EntityFrameworkCore;
using MiniGrc.Domain;
using MiniGrc.Domain.Entities;
using MiniGrc.Domain.Enums;
using MiniGrc.Domain.Repositories;
using MiniGrc.Infrastructure.Persistence;

namespace MiniGrc.Infrastructure.Repositories;

/// <summary>EF Core implementation of <see cref="IControlRepository"/>.</summary>
public sealed class ControlRepository : IControlRepository
{
    private readonly MiniGrcDbContext _db;

    /// <summary>Constructs the repository with the EF Core context.</summary>
    public ControlRepository(MiniGrcDbContext db) => _db = db;

    /// <inheritdoc/>
    public Task<Control?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Controls.Include(c => c.Evidence).FirstOrDefaultAsync(c => c.Id == id, ct);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Control>> GetAllAsync(ComplianceFramework? framework = null, CancellationToken ct = default)
    {
        var query = _db.Controls.Include(c => c.Evidence).AsQueryable();
        if (framework.HasValue) query = query.Where(c => c.Framework == framework.Value);
        return await query.OrderBy(c => c.Code).ToListAsync(ct);
    }

    /// <inheritdoc/>
    public Task<Control?> GetByCodeAsync(string code, CancellationToken ct = default)
        => _db.Controls.FirstOrDefaultAsync(c => c.Code == code, ct);

    /// <inheritdoc/>
    public async Task AddAsync(Control control, CancellationToken ct = default)
    {
        await _db.Controls.AddAsync(control, ct);
    }

    /// <inheritdoc/>
    public Task UpdateAsync(Control control, CancellationToken ct = default)
    {
        _db.Controls.Update(control);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(Control control, CancellationToken ct = default)
    {
        _db.Controls.Remove(control);
        await Task.CompletedTask;
    }
}

/// <summary>EF Core implementation of <see cref="IFindingRepository"/>.</summary>
public sealed class FindingRepository : IFindingRepository
{
    private readonly MiniGrcDbContext _db;

    /// <summary>Constructs the repository with the EF Core context.</summary>
    public FindingRepository(MiniGrcDbContext db) => _db = db;

    /// <inheritdoc/>
    public Task<Finding?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Findings.Include(f => f.RemediationTasks).FirstOrDefaultAsync(f => f.Id == id, ct);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Finding>> GetAllAsync(bool onlyUnmapped = false, CancellationToken ct = default)
    {
        var query = _db.Findings.Include(f => f.RemediationTasks).AsQueryable();
        if (onlyUnmapped) query = query.Where(f => !f.Mapped);
        return await query.OrderByDescending(f => f.CreatedAtUtc).ToListAsync(ct);
    }

    /// <inheritdoc/>
    public Task<Finding?> GetByExternalIdAsync(string externalId, CancellationToken ct = default)
        => _db.Findings.FirstOrDefaultAsync(f => f.ExternalId == externalId, ct);

    /// <inheritdoc/>
    public async Task AddAsync(Finding finding, CancellationToken ct = default)
    {
        await _db.Findings.AddAsync(finding, ct);
    }

    /// <inheritdoc/>
    public Task UpdateAsync(Finding finding, CancellationToken ct = default)
    {
        _db.Findings.Update(finding);
        return Task.CompletedTask;
    }
}

/// <summary>EF Core implementation of <see cref="IRiskRepository"/>.</summary>
public sealed class RiskRepository : IRiskRepository
{
    private readonly MiniGrcDbContext _db;

    /// <summary>Constructs the repository with the EF Core context.</summary>
    public RiskRepository(MiniGrcDbContext db) => _db = db;

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Risk>> GetAllAsync(CancellationToken ct = default)
        => await _db.Risks.OrderByDescending(r => r.Likelihood * r.Impact).ToListAsync(ct);

    /// <inheritdoc/>
    public async Task UpsertAsync(Risk risk, CancellationToken ct = default)
    {
        var existing = await _db.Risks.FindAsync([risk.Id], ct);
        if (existing is null) await _db.Risks.AddAsync(risk, ct);
        else _db.Risks.Update(risk);
    }

    /// <inheritdoc/>
    public Task DeleteAsync(Risk risk, CancellationToken ct = default)
    {
        _db.Risks.Remove(risk);
        return Task.CompletedTask;
    }
}

/// <summary>
/// EF Core implementation of <see cref="IUnitOfWork"/>. Bundles the three repositories behind a
/// single <c>SaveChangesAsync</c> so handlers persist atomically.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly MiniGrcDbContext _db;

    /// <summary>Constructs the unit of work and its repository ports.</summary>
    public UnitOfWork(MiniGrcDbContext db)
    {
        _db = db;
        Controls = new ControlRepository(db);
        Findings = new FindingRepository(db);
        Risks = new RiskRepository(db);
    }

    /// <inheritdoc/>
    public IControlRepository Controls { get; }

    /// <inheritdoc/>
    public IFindingRepository Findings { get; }

    /// <inheritdoc/>
    public IRiskRepository Risks { get; }

    /// <inheritdoc/>
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}

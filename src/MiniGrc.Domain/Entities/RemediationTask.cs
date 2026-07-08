using MiniGrc.Domain.Common;
using MiniGrc.Domain.Enums;

namespace MiniGrc.Domain.Entities;

/// <summary>
/// A remediation task drafted by the AI agent to close a <see cref="Finding"/>. Child entity of
/// <see cref="Finding"/>.
/// </summary>
public sealed class RemediationTask : Entity
{
    /// <summary>Action the owner must take.</summary>
    public string Title { get; private set; }

    /// <summary>Optional detail / acceptance criteria.</summary>
    public string? Detail { get; private set; }

    /// <summary>Work priority.</summary>
    public RemediationPriority Priority { get; private set; }

    /// <summary>Foreign key to the owning <see cref="Finding"/>.</summary>
    public Guid FindingId { get; private set; }

    private RemediationTask()
    {
        Title = string.Empty;
    }

    private RemediationTask(string title, string? detail, RemediationPriority priority, Guid findingId)
    {
        Title = title;
        Detail = detail;
        Priority = priority;
        FindingId = findingId;
    }

    /// <summary>Factory validating the task attributes.</summary>
    public static RemediationTask Create(string title, string? detail, RemediationPriority priority, Guid findingId)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Task title is required.", nameof(title));
        return new RemediationTask(title.Trim(), detail?.Trim(), priority, findingId);
    }
}

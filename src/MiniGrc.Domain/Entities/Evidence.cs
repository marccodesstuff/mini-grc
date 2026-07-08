using MiniGrc.Domain.Common;
using MiniGrc.Domain.Enums;

namespace MiniGrc.Domain.Entities;

/// <summary>
/// An evidence artifact (a policy PDF, a scanner export, a screenshot) uploaded to prove a
/// control operates. Child entity of <see cref="Control"/>; never persisted on its own.
/// </summary>
public sealed class Evidence : Entity
{
    /// <summary>Original file name as uploaded.</summary>
    public string FileName { get; private set; }

    /// <summary>MIME type of the uploaded content.</summary>
    public string ContentType { get; private set; }

    /// <summary>Size of the artifact in bytes.</summary>
    public long SizeBytes { get; private set; }

    /// <summary>Person or system that uploaded the artifact.</summary>
    public string UploadedBy { get; private set; }

    /// <summary>Review state of the artifact.</summary>
    public EvidenceStatus Status { get; private set; }

    /// <summary>Reviewer who approved/rejected the artifact, if any.</summary>
    public string? Reviewer { get; private set; }

    /// <summary>Foreign key to the owning <see cref="Control"/>.</summary>
    public Guid ControlId { get; private set; }

    private Evidence()
    {
        FileName = string.Empty;
        ContentType = string.Empty;
        UploadedBy = string.Empty;
    }

    private Evidence(string fileName, string contentType, long sizeBytes, string uploadedBy, Guid controlId)
    {
        FileName = fileName;
        ContentType = contentType;
        SizeBytes = sizeBytes;
        UploadedBy = uploadedBy;
        ControlId = controlId;
        Status = EvidenceStatus.PendingReview;
    }

    /// <summary>Factory that validates the upload metadata.</summary>
    public static Evidence Create(string fileName, string contentType, long sizeBytes, string uploadedBy, Guid controlId)
    {
        if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("File name is required.", nameof(fileName));
        if (sizeBytes <= 0) throw new ArgumentOutOfRangeException(nameof(sizeBytes), "Size must be positive.");

        return new Evidence(fileName.Trim(), contentType.Trim(), sizeBytes, uploadedBy.Trim(), controlId);
    }

    /// <summary>Sets the review outcome for this evidence artifact.</summary>
    public void SetStatus(EvidenceStatus status, string? reviewer = null)
    {
        Status = status;
        Reviewer = reviewer?.Trim();
        Touch();
    }
}

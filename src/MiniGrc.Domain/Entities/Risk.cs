using MiniGrc.Domain.Common;
using MiniGrc.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniGrc.Domain.Entities;

/// <summary>
/// A risk register entry. Risks may be identified manually or derived by the agent from the set
/// of open findings. Residual severity is computed from likelihood x impact.
/// </summary>
public sealed class Risk : Entity, IAggregateRoot
{
    /// <summary>Risk title.</summary>
    public string Title { get; private set; }

    /// <summary>Description of the risk and its drivers.</summary>
    public string? Description { get; private set; }

    /// <summary>Likelihood score 1-5.</summary>
    public int Likelihood { get; private set; }

    /// <summary>Impact score 1-5.</summary>
    public int Impact { get; private set; }

    /// <summary>Derived residual severity from <see cref="Likelihood"/> x <see cref="Impact"/>.</summary>
    [NotMapped]
    public RiskSeverity Severity => DeriveSeverity(Likelihood * Impact);

    /// <summary>Whether the risk has been accepted, mitigated, or is open.</summary>
    public bool Accepted { get; private set; }

    private Risk()
    {
        Title = string.Empty;
    }

    private Risk(string title, string? description, int likelihood, int impact)
    {
        Title = title;
        Description = description;
        Likelihood = likelihood;
        Impact = impact;
    }

    /// <summary>Factory that validates the 1-5 scoring bounds.</summary>
    public static Risk Create(string title, string? description, int likelihood, int impact)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Risk title is required.", nameof(title));
        if (likelihood is < 1 or > 5) throw new ArgumentOutOfRangeException(nameof(likelihood), "Likelihood must be 1-5.");
        if (impact is < 1 or > 5) throw new ArgumentOutOfRangeException(nameof(impact), "Impact must be 1-5.");

        return new Risk(title.Trim(), description?.Trim(), likelihood, impact);
    }

    /// <summary>Marks the risk as accepted by management.</summary>
    public void Accept() { Accepted = true; Touch(); }

    private static RiskSeverity DeriveSeverity(int score) => score switch
    {
        <= 4 => RiskSeverity.Negligible,
        <= 9 => RiskSeverity.Low,
        <= 14 => RiskSeverity.Moderate,
        <= 20 => RiskSeverity.High,
        _ => RiskSeverity.Extreme
    };
}

using Microsoft.AspNetCore.Components;

namespace Augur.Web.Components.Layout;

/// <summary>Per-page metadata pushed to the layout top bar via a cascading value.</summary>
public sealed record PageMeta(string Title, string Crumb)
{
    /// <summary>Default metadata used before a page overrides it.</summary>
    public static readonly PageMeta Default = new("Mini-GRC", "Compliance dashboard");
}


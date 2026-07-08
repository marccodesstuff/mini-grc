using System.Xml;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace MiniGrc.Api.OpenApi;

/// <summary>
/// OpenAPI transformer that pulls the XML documentation comments written on controllers and
/// endpoints and embeds them in the generated 3.1.1 document (operation summaries/descriptions
/// and schema descriptions). The native Microsoft.AspNetCore.OpenApi generator does not read XML
/// comments automatically, so this makes the "mandatory XML documentation" requirement visible in
/// the spec itself.
/// </summary>
public sealed class XmlCommentsTransformer : IOpenApiOperationTransformer, IOpenApiSchemaTransformer
{
    private readonly Dictionary<string, string> _summaries = new();

    /// <summary>Loads the assembly's XML documentation file into memory.</summary>
    public XmlCommentsTransformer(string xmlPath)
    {
        if (!File.Exists(xmlPath)) return;
        var doc = new XmlDocument();
        doc.Load(xmlPath);
        foreach (XmlNode member in doc.SelectNodes("/doc/members/member")!)
        {
            var name = member.Attributes?["name"]?.Value;
            if (name is null) continue;
            var summary = member.SelectSingleNode("summary")?.InnerText.Trim();
            if (!string.IsNullOrWhiteSpace(summary))
                _summaries[name] = Normalize(summary!);
        }
    }

    /// <inheritdoc/>
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken ct)
    {
        var displayName = context.Description.ActionDescriptor.DisplayName ?? string.Empty;
        if (TryFindSummary(displayName, out var text))
        {
            operation.Summary ??= text;
            operation.Description ??= text;
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken ct)
    {
        var type = context.JsonTypeInfo.Type;
        var key = $"T:{type.FullName}";
        if (_summaries.TryGetValue(key, out var text))
            schema.Description ??= text;
        return Task.CompletedTask;
    }

    private bool TryFindSummary(string displayName, out string text)
    {
        // displayName e.g. "MiniGrc.Api.Controllers.ControlsController.GetAll (MiniGrc.Api)".
        var shortName = displayName.Split('(')[0].Trim(); // ControlsController.GetAll
        var full = $"M:MiniGrc.Api.Controllers.{shortName}";
        if (_summaries.TryGetValue(full, out var found))
        {
            text = found;
            return true;
        }
        text = string.Empty;
        return false;
    }

    private static string Normalize(string s) => string.Join(" ", s.Split(['\n', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries));
}

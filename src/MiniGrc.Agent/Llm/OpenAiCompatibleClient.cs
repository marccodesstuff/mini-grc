using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace MiniGrc.Agent.Llm;

/// <summary>
/// Minimal OpenAI-compatible chat client. Talks to any endpoint exposing
/// <c>/v1/chat/completions</c> (OpenAI, LM Studio, Ollama, etc.). Kept dependency-free so the
/// agent layer has no hard coupling to a single vendor.
/// </summary>
public sealed class OpenAiCompatibleClient
{
    private readonly HttpClient _http;
    private readonly string _model;

    /// <summary>Constructs the client.</summary>
    /// <param name="httpClient">HTTP client (base address set by DI).</param>
    /// <param name="model">Model name to request.</param>
    public OpenAiCompatibleClient(HttpClient httpClient, string model)
    {
        _http = httpClient;
        _model = model;
    }

    /// <summary>Sends a chat completion request and returns the assistant message content.</summary>
    /// <param name="systemPrompt">System instructions.</param>
    /// <param name="userPrompt">User content.</param>
    /// <param name="temperature">Sampling temperature.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="InvalidOperationException">When the endpoint is unreachable (caller falls back).</exception>
    public async Task<string> CompleteAsync(string systemPrompt, string userPrompt, double temperature = 0.2, CancellationToken ct = default)
    {
        var payload = new
        {
            model = _model,
            temperature,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            }
        };

        using var response = await _http.PostAsJsonAsync("v1/chat/completions", payload, ct);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException($"LLM endpoint returned {(int)response.StatusCode}: {body}");
        }

        using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
        var content = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();
        return content ?? string.Empty;
    }
}

/// <summary>Response shapes for deserialization of the chat completion payload.</summary>
file sealed class ChatResponse
{
    public List<ChatChoice>? choices { get; set; }
    public sealed class ChatChoice
    {
        public ChatMessage? message { get; set; }
    }
    public sealed class ChatMessage
    {
        public string? content { get; set; }
    }
}

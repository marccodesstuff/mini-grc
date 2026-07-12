using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace MiniGrc.Api.Auth;

/// <summary>
/// Minimal API-key authentication. Clients must present a shared secret in the
/// <c>X-Api-Key</c> header; the expected value comes from <c>Auth:ApiKey</c> configuration.
/// The key is compared in constant time so a wrong key cannot be brute-forced via timing.
/// </summary>
public sealed class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    /// <summary>The authentication scheme name registered in <c>Program</c>.</summary>
    public const string SchemeName = "ApiKey";

    /// <summary>Header carrying the shared secret.</summary>
    public const string HeaderName = "X-Api-Key";

    private readonly string? _expectedKey;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IConfiguration configuration)
        : base(options, logger, encoder)
    {
        _expectedKey = configuration["Auth:ApiKey"];
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Misconfiguration: no server key set. Fail closed rather than allowing everything.
        if (string.IsNullOrEmpty(_expectedKey))
            return Task.FromResult(AuthenticateResult.Fail("API key authentication is not configured."));

        if (!Request.Headers.TryGetValue(HeaderName, out var provided) || string.IsNullOrEmpty(provided))
            return Task.FromResult(AuthenticateResult.NoResult());

        var providedBytes = Encoding.UTF8.GetBytes(provided.ToString());
        var expectedBytes = Encoding.UTF8.GetBytes(_expectedKey);
        if (!CryptographicOperations.FixedTimeEquals(providedBytes, expectedBytes))
            return Task.FromResult(AuthenticateResult.Fail("Invalid API key."));

        var identity = new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.Name, "api-client") }, SchemeName);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

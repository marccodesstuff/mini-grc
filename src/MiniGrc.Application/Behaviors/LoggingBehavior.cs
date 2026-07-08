using MediatR;

namespace MiniGrc.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior that logs each request and its latency. Kept lightweight (no external
/// logger dependency) by writing to the standard console sink; in production this would be
/// structured logging via ILogger.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <summary>Invoked by MediatR; times the inner handler and logs the result.</summary>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var name = typeof(TRequest).Name;
        var start = DateTime.UtcNow;
        var response = await next();
        var elapsed = (DateTime.UtcNow - start).TotalMilliseconds;
        Console.WriteLine($"[MediatR] {name} handled in {elapsed:F1} ms");
        return response;
    }
}

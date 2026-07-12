using MediatR;

namespace Augur.Application.Behaviors;

/// <remarks>
/// Logs each request type and its latency. Uses Console instead of ILogger to avoid adding an
/// external logging dependency; swap to structured logging in production.
/// </remarks>
public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
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

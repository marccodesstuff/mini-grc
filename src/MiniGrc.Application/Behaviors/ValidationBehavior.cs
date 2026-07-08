using FluentValidation;
using MediatR;

namespace MiniGrc.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior that runs FluentValidation validators before the handler executes.
/// Any validation failure is thrown as <see cref="ValidationException"/>, short-circuiting the
/// pipeline so handlers only ever see valid input.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    /// <summary>Constructs the behavior with the registered validators for <typeparamref name="TRequest"/>.</summary>
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators) => _validators = validators;

    /// <summary>Invoked by MediatR; runs validators then continues the pipeline.</summary>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var failures = (await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)))).SelectMany(r => r.Errors)
                .Where(f => f is not null).ToList();

            if (failures.Count != 0)
                throw new ValidationException(failures);
        }

        return await next();
    }
}

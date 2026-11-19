using FluentValidation;
using MediatR;

namespace Citas.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!validators.Any()) return await next();

        var context = new ValidationContext<TRequest>(request);
        var resultados = await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)));
        var errores = resultados.SelectMany(r => r.Errors).Where(e => e is not null).ToList();

        if (errores.Count > 0) throw new ValidationException(errores);

        return await next();
    }
}

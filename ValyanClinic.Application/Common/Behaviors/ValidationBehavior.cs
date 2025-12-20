using FluentValidation;
using MediatR;

namespace ValyanClinic.Application.Common.Behaviors;

/// <summary>
/// MediatR Pipeline Behavior pentru validare automata cu FluentValidation
/// Intercepteaza toate request-urile (Commands si Queries) si ruleaza validatorii INAINTE de handler
/// Daca validarea esueaza, arunca ValidationException cu toate erorile
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Daca nu exista validatori pentru acest request type, skip validation
        if (!_validators.Any())
        {
            return await next();
        }

        // Context pentru validare
        var context = new ValidationContext<TRequest>(request);

        // Ruleaza TOTI validatorii in paralel pentru performance
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        // Colecteaza toate erorile de la toti validatorii
        var failures = validationResults
            .Where(r => !r.IsValid)
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        // Daca exista erori, arunca ValidationException
        if (failures.Count != 0)
        {
            throw new ValidationException(failures);
        }

        // Validare OK - continua cu handler-ul
        return await next();
    }
}

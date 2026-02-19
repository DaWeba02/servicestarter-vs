using FluentValidation;
using FluentValidation.Results;

namespace ServiceStarter.Api.Common.Validation;

public sealed class ValidationFilter<TRequest>(IValidator<TRequest> validator) : IEndpointFilter
{
    private readonly IValidator<TRequest> _validator = validator;

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var request = context.Arguments.OfType<TRequest>().FirstOrDefault();

        if (request is null)
        {
            return Results.BadRequest(new { error = "Request body is required" });
        }

        ValidationResult validationResult = await _validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.ToDictionary();
            return Results.ValidationProblem(errors);
        }

        return await next(context);
    }
}

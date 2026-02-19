using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ServiceStarter.Api.Common.ErrorHandling;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation failed");
            await WriteProblemAsync(context, StatusCodes.Status400BadRequest, "Request validation failed",
                errors: ex.Errors.ToDictionary(failure => failure.PropertyName, failure => new[] { failure.ErrorMessage }));
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database update failed");
            await WriteProblemAsync(context, StatusCodes.Status500InternalServerError, "A database error occurred");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteProblemAsync(context, StatusCodes.Status500InternalServerError, "An unexpected error occurred");
        }
    }

    private static Task WriteProblemAsync(
        HttpContext context,
        int statusCode,
        string title,
        IDictionary<string, string[]>? errors = null)
    {
        if (context.Response.HasStarted)
        {
            return Task.CompletedTask;
        }

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = $"https://httpstatuses.com/{statusCode}"
        };

        if (errors is not null)
        {
            problem.Extensions["errors"] = errors;
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        return JsonSerializer.SerializeAsync(context.Response.Body, problem);
    }
}

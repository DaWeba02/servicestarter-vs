using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using ServiceStarter.Api.Common.Validation;
using ServiceStarter.Api.Infrastructure.Auth;

namespace ServiceStarter.Api.Features.Auth.Login;

public static class LoginEndpoints
{
    public static RouteGroupBuilder MapLoginEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/auth")
            .WithTags("Auth");

        group.MapPost("/login", HandleLogin)
            .AddEndpointFilter<ValidationFilter<LoginRequest>>();

        return group;
    }

    private static Results<Ok<LoginResponse>, UnauthorizedHttpResult> HandleLogin(
        LoginRequest request,
        ITokenService tokenService)
    {
        // Basic role rule based on email domain.
        var role = request.Email.EndsWith("@admin.local", StringComparison.OrdinalIgnoreCase)
            ? AuthRoles.Admin
            : AuthRoles.User;

        // Password check intentionally minimal for starter template.
        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return TypedResults.Unauthorized();
        }

        var token = tokenService.GenerateToken(request.Email, role);
        return TypedResults.Ok(new LoginResponse(token));
    }
}
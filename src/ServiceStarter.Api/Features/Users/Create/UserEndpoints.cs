using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceStarter.Api.Common.Validation;
using ServiceStarter.Api.Infrastructure.Auth;
using ServiceStarter.Api.Infrastructure.Persistence;
using ServiceStarter.Api.Infrastructure.Persistence.Entities;

namespace ServiceStarter.Api.Features.Users.Create;

public static class UserEndpoints
{
    public static RouteGroupBuilder MapUserEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/users")
            .WithTags("Users")
            .RequireAuthorization(AuthPolicies.AdminOnly);

        group.MapPost("/", HandleCreateUser)
            .AddEndpointFilter<ValidationFilter<CreateUserRequest>>()
            .Produces<CreateUserResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return group;
    }

    private static async Task<Results<Created<CreateUserResponse>, Conflict<ProblemDetails>>> HandleCreateUser(
        CreateUserRequest request,
        AppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var exists = await dbContext.Users.AnyAsync(u => u.Email == request.Email, cancellationToken);

        if (exists)
        {
            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Email already exists",
                Type = "https://httpstatuses.com/409"
            };

            return TypedResults.Conflict(problem);
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            DisplayName = request.DisplayName,
            Role = AuthRoles.User
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Created($"/users/{user.Id}", new CreateUserResponse(user.Id));
    }
}
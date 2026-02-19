namespace ServiceStarter.Api.Infrastructure.Auth;

public static class AuthRoles
{
    public const string Admin = "Admin";
    public const string User = "User";
}

public static class AuthPolicies
{
    public const string AdminOnly = "AdminOnly";
}

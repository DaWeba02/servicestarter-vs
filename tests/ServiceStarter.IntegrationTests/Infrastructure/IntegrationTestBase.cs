using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServiceStarter.Api.Infrastructure.Persistence;
using Xunit;

namespace ServiceStarter.IntegrationTests.Infrastructure;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly SqlServerContainerFixture Fixture;
    protected readonly TestWebApplicationFactory Factory;
    protected readonly HttpClient Client;

    protected IntegrationTestBase(SqlServerContainerFixture fixture, string environment = "Development")
    {
        Fixture = fixture;
        Factory = new TestWebApplicationFactory(fixture, environment);
        Client = Factory.CreateClient();
    }

    public virtual async Task InitializeAsync()
    {
        await ResetDatabaseAsync();
    }

    public virtual async Task DisposeAsync()
    {
        Client.Dispose();
        await Factory.DisposeAsync();
    }

    protected async Task ResetDatabaseAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        var applied = await db.Database.GetAppliedMigrationsAsync();
        if (!applied.Any())
        {
            var conn = db.Database.GetDbConnection().ConnectionString;
            throw new InvalidOperationException($"No migrations applied. Connection: {conn}");
        }

        await db.Database.ExecuteSqlRawAsync("DELETE FROM [Users]");
    }

    protected async Task<string> AuthenticateAsync(string email)
    {
        var response = await Client.PostAsJsonAsync("/auth/login", new ServiceStarter.Api.Features.Auth.Login.LoginRequest(email, "password123"));
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<ServiceStarter.Api.Features.Auth.Login.LoginResponse>()
                      ?? throw new InvalidOperationException("Login response was empty");

        return payload.AccessToken;
    }
}

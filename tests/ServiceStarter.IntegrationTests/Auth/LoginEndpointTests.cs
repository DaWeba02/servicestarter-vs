using System.Net.Http.Json;
using FluentAssertions;
using ServiceStarter.Api.Features.Auth.Login;
using ServiceStarter.IntegrationTests.Infrastructure;
using Xunit;

namespace ServiceStarter.IntegrationTests.Auth;

[Collection(IntegrationTestCollection.CollectionName)]
public sealed class LoginEndpointTests(SqlServerContainerFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Login_ReturnsToken()
    {
        var response = await Client.PostAsJsonAsync("/auth/login", new LoginRequest("a@b.com", "password123"));

        response.IsSuccessStatusCode.Should().BeTrue();

        var payload = await response.Content.ReadFromJsonAsync<LoginResponse>();
        payload?.AccessToken.Should().NotBeNullOrEmpty();
    }
}

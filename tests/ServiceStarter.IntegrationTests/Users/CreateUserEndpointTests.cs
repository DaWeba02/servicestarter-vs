using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using ServiceStarter.Api.Features.Users.Create;
using ServiceStarter.IntegrationTests.Infrastructure;
using Xunit;

namespace ServiceStarter.IntegrationTests.Users;

[Collection(IntegrationTestCollection.CollectionName)]
public sealed class CreateUserEndpointTests(SqlServerContainerFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task CreateUser_WithoutToken_ReturnsUnauthorized()
    {
        var response = await Client.PostAsJsonAsync("/users", new CreateUserRequest("user@example.com", "Jane Doe"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateUser_WithNonAdminToken_ReturnsForbidden()
    {
        var token = await AuthenticateAsync("a@b.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.PostAsJsonAsync("/users", new CreateUserRequest("user-nonadmin@example.com", "John Doe"));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateUser_WithAdminToken_ReturnsCreated()
    {
        var token = await AuthenticateAsync("x@admin.local");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.PostAsJsonAsync("/users", new CreateUserRequest("user1@example.com", "Jane Doe"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var payload = await response.Content.ReadFromJsonAsync<CreateUserResponse>();
        payload?.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateUser_DuplicateEmail_ReturnsConflict()
    {
        var token = await AuthenticateAsync("x@admin.local");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new CreateUserRequest("dupe@example.com", "Jane Doe");

        var first = await Client.PostAsJsonAsync("/users", request);
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        var duplicate = await Client.PostAsJsonAsync("/users", request);
        duplicate.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}

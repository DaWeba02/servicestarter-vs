using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiceStarter.Api.Features.Auth.Login;
using ServiceStarter.IntegrationTests.Infrastructure;
using Xunit;

namespace ServiceStarter.IntegrationTests;

[Collection(IntegrationTestCollection.CollectionName)]
public sealed class ProblemDetailsTests : IntegrationTestBase
{
    public ProblemDetailsTests(SqlServerContainerFixture fixture)
        : base(fixture, environment: "Testing")
    {
    }

    [Fact]
    public async Task ValidationErrors_ReturnProblemDetails()
    {
        var response = await Client.PostAsJsonAsync("/auth/login", new LoginRequest("not-an-email", "123"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");

        var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Status.Should().Be((int)HttpStatusCode.BadRequest);
        problem.Errors.Should().ContainKey(nameof(LoginRequest.Email));
    }

    [Fact]
    public async Task UnhandledExceptions_ReturnProblemDetails500()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/ping");
        request.Headers.Add("X-Debug-Throw", "true");

        var response = await Client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Status.Should().Be((int)HttpStatusCode.InternalServerError);
        problem.Title.Should().Be("An unexpected error occurred");
        problem.Type.Should().Be("https://httpstatuses.com/500");
    }
}
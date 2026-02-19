using System.Net;
using FluentAssertions;
using ServiceStarter.IntegrationTests.Infrastructure;
using Xunit;

namespace ServiceStarter.IntegrationTests;

[Collection(IntegrationTestCollection.CollectionName)]
public sealed class SwaggerTests(SqlServerContainerFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task SwaggerJson_IsAvailable_InDevelopment()
    {
        var response = await Client.GetAsync("/swagger/v1/swagger.json");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SwaggerJson_IsNotExposed_InProduction()
    {
        await using var productionFactory = new TestWebApplicationFactory(fixture, "Production");
        using var productionClient = productionFactory.CreateClient();

        var response = await productionClient.GetAsync("/swagger/v1/swagger.json");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
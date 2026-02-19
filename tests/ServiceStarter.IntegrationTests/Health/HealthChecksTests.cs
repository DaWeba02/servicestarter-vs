using System.Net;
using FluentAssertions;
using ServiceStarter.IntegrationTests.Infrastructure;
using Xunit;

namespace ServiceStarter.IntegrationTests.Health;

[Collection(IntegrationTestCollection.CollectionName)]
public sealed class HealthChecksTests(SqlServerContainerFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Live_Returns200()
    {
        var response = await Client.GetAsync("/health/live");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Ready_Returns200_WhenDatabaseIsReady()
    {
        await WaitForReadyAsync();

        var response = await Client.GetAsync("/health/ready");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private async Task WaitForReadyAsync(TimeSpan? timeout = null)
    {
        var stopAt = DateTime.UtcNow + (timeout ?? TimeSpan.FromSeconds(120));
        HttpStatusCode? lastStatus = null;
        string? lastBody = null;

        while (DateTime.UtcNow < stopAt)
        {
            var response = await Client.GetAsync("/health/ready");
            lastStatus = response.StatusCode;
            lastBody = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return;
            }

            await Task.Delay(500);
        }

        throw new TimeoutException($"Health check did not return 200 within the expected time window. LastStatus={lastStatus}, LastBody={lastBody}");
    }
}

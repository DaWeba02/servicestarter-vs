using Xunit;

namespace ServiceStarter.IntegrationTests.Infrastructure;

[CollectionDefinition(CollectionName)]
public sealed class IntegrationTestCollection : ICollectionFixture<SqlServerContainerFixture>
{
    public const string CollectionName = "integration";
}

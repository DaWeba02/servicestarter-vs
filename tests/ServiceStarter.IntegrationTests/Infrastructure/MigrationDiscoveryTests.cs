using System.Reflection;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using ServiceStarter.Api.Infrastructure.Persistence;
using Xunit;

namespace ServiceStarter.IntegrationTests.Infrastructure;

[Collection(IntegrationTestCollection.CollectionName)]
public sealed class MigrationDiscoveryTests(SqlServerContainerFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact]
    public void ShouldDiscoverMigrationTypesInAssembly()
    {
        var assembly = typeof(AppDbContext).Assembly;

        var migrationTypes = assembly
            .GetTypes()
            .Where(t => t.IsSubclassOf(typeof(Migration)) && !t.IsAbstract)
            .ToList();

        migrationTypes.Should().NotBeEmpty(
            "EF Core migrations were not discovered in the assembly.");
    }

    [Fact]
    public void ShouldReturnMigrationsFromDatabaseFacade()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var migrations = db.Database.GetMigrations();

        migrations.Should().NotBeEmpty(
            "EF Core migrations were not discovered in the assembly.");
    }
}
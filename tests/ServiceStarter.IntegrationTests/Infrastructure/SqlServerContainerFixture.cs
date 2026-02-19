using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ServiceStarter.Api.Infrastructure.Persistence;
using DotNet.Testcontainers.Builders;
using Testcontainers.MsSql;
using Xunit;

namespace ServiceStarter.IntegrationTests.Infrastructure;

public sealed class SqlServerContainerFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container;

    public SqlServerContainerFixture()
    {
        _container = new MsSqlBuilder()
            .WithPassword("Your_password123")
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithEnvironment("ACCEPT_EULA", "Y")
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilMessageIsLogged("SQL Server is now ready for client connections"))
            .WithCleanUp(true)
            .Build();
    }

    public string ConnectionString
    {
        get
        {
            var builder = new SqlConnectionStringBuilder(_container.GetConnectionString());

            if (builder.DataSource.Contains("localhost", StringComparison.OrdinalIgnoreCase))
            {
                builder.DataSource = builder.DataSource.Replace("localhost", "127.0.0.1", StringComparison.OrdinalIgnoreCase);
            }

            builder.InitialCatalog = "servicestarter";
            builder.TrustServerCertificate = true;
            return builder.ToString();
        }
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await EnsureDatabaseExistsAsync();
        await ApplyMigrationsAsync();
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();

    private async Task EnsureDatabaseExistsAsync()
    {
        // Retry a few times in case SQL Server isn't fully ready even after wait strategy.
        var retries = 5;
        var delayMs = 1000;
        Exception? last = null;

        for (var i = 0; i < retries; i++)
        {
            try
            {
                using var connection = new SqlConnection(_container.GetConnectionString());
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = "IF DB_ID('servicestarter') IS NULL CREATE DATABASE [servicestarter];";
                await command.ExecuteNonQueryAsync();
                return;
            }
            catch (Exception ex)
            {
                last = ex;
                await Task.Delay(delayMs);
                delayMs *= 2;
            }
        }

        throw last ?? new InvalidOperationException("Failed to ensure database exists.");
    }

    private async Task ApplyMigrationsAsync()
    {
        var migrationsAssembly = typeof(AppDbContext).Assembly.GetName().Name;
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(ConnectionString, sql => sql.MigrationsAssembly(migrationsAssembly))
            .Options;

        await using var db = new AppDbContext(options);
        var pending = await db.Database.GetPendingMigrationsAsync();
        var appliedBefore = await db.Database.GetAppliedMigrationsAsync();

        await db.Database.MigrateAsync();

        var appliedAfter = await db.Database.GetAppliedMigrationsAsync();

        if (!appliedAfter.Any())
        {
            var message = $"No migrations applied. Assembly: {migrationsAssembly}. " +
                          $"Pending before: [{string.Join(", ", pending)}]; " +
                          $"Applied before: [{string.Join(", ", appliedBefore)}]; " +
                          $"Applied after: [{string.Join(", ", appliedAfter)}]; " +
                          $"Connection: {db.Database.GetConnectionString()}";
            throw new InvalidOperationException(message);
        }
    }
}

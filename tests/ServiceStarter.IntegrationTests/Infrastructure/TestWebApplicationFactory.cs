using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ServiceStarter.Api;
using ServiceStarter.Api.Infrastructure.Persistence;
using ServiceStarter.Api.Common.Health;

namespace ServiceStarter.IntegrationTests.Infrastructure;

public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqlServerContainerFixture _fixture;
    private readonly string _environment;

    public TestWebApplicationFactory(SqlServerContainerFixture fixture, string environment = "Development")
    {
        _fixture = fixture;
        _environment = environment;
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment(_environment);

        builder.ConfigureAppConfiguration(config =>
        {
            var settings = new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = _fixture.ConnectionString
            };
            config.AddInMemoryCollection(settings!);
        });

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    _fixture.ConnectionString,
                    sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.GetName().Name)));

            // Override health checks to point at the test container connection string.
            services.Configure<HealthCheckServiceOptions>(options => options.Registrations.Clear());
            services.AddHealthChecks()
                .AddSqlServer(_fixture.ConnectionString, name: HealthCheckNames.Sql);
        });

        var host = base.CreateHost(builder);

        // Apply migrations against the container database before tests run.
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();

        return host;
    }
}

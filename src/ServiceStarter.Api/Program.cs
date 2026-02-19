using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using ServiceStarter.Api.Common.ErrorHandling;
using ServiceStarter.Api.Common.Health;
using ServiceStarter.Api.Common.Validation;
using ServiceStarter.Api.Features.Auth.Login;
using ServiceStarter.Api.Features.Users.Create;
using ServiceStarter.Api.Infrastructure.Auth;
using ServiceStarter.Api.Infrastructure.Persistence;

namespace ServiceStarter.Api;

public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Host.UseSerilog((ctx, lc) => lc
            .ReadFrom.Configuration(ctx.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console());

        var configuration = builder.Configuration;

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        builder.Services.AddScoped<ITokenService, JwtTokenService>();

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("Default")));

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
                                ?? throw new InvalidOperationException("Jwt configuration is missing");

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey))
                };
            });

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthPolicies.AdminOnly, policy => policy.RequireRole(AuthRoles.Admin));
        });

        builder.Services.AddHealthChecks()
            .AddSqlServer(configuration.GetConnectionString("Default")!, name: HealthCheckNames.Sql);

        builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
        builder.Services.AddTransient(typeof(ValidationFilter<>));

        builder.Services.AddProblemDetails();

        var app = builder.Build();
        var environment = app.Environment;

        app.UseSerilogRequestLogging();
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapGet("/ping", (HttpContext context) =>
        {
            if (environment.IsEnvironment("Testing") &&
                context.Request.Headers.TryGetValue("X-Debug-Throw", out var throwHeader) &&
                throwHeader.Any(value => value.Equals("true", StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("Synthetic test exception");
            }

            return Results.Ok(new { status = "ok" });
        });

        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false,
            ResponseWriter = HealthResponseWriters.WriteMinimalJson
        });

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = registration => registration.Name == HealthCheckNames.Sql,
            ResponseWriter = HealthResponseWriters.WriteMinimalJson
        });

        app.MapLoginEndpoints();
        app.MapUserEndpoints();

        var applyMigrationsFlag = configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup");
        if (environment.IsDevelopment() || environment.IsEnvironment("Testing") || applyMigrationsFlag)
        {
            app.ApplyMigrations();
        }

        app.Run();
    }
}

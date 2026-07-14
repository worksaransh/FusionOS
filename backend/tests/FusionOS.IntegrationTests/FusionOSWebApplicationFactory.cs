using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;
using Xunit;

namespace FusionOS.IntegrationTests;

/// <summary>
/// Boots the real Host (FusionOS.Api.Host's Program) against a disposable
/// Postgres container (Testcontainers) instead of mocks — the integration test
/// scaffolding the enterprise audit flagged as entirely missing. Requires
/// Docker to be available wherever these tests run (the same requirement as
/// docker-compose.yml for local dev); there is no in-sandbox way to verify
/// this here since this environment has neither Docker nor the .NET SDK — see
/// README known-gaps. Runs the Host in the "Testing" environment so
/// Program.cs's fail-fast secret check (07_SECURITY.md) does not fire.
/// </summary>
public sealed class FusionOSWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlBuilder _postgresBuilder = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("fusionos_test")
        .WithUsername("fusionos")
        .WithPassword("fusionos");

    private PostgreSqlContainer? _postgres;

    public async Task InitializeAsync()
    {
        _postgres = _postgresBuilder.Build();
        await _postgres.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        if (_postgres is not null)
            await _postgres.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] = _postgres?.GetConnectionString(),
            });
        });
    }
}

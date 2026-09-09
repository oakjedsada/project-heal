using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;
using Xunit;

namespace MindCheck.Tests.Integration;

// Boots the real Api against a throwaway Postgres container (not a fake/mock
// store) so these tests exercise the actual EF Core mapping, migrations, and
// seed data, not just in-memory approximations of them.
public sealed class MindCheckApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("mindcheck")
        .WithUsername("mindcheck")
        .WithPassword("mindcheck")
        .Build();

    public const string TestAdminUsername = "integration-test-admin";
    public const string TestAdminPassword = "integration-test-admin-password";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:MindCheck"] = _container.GetConnectionString(),
                ["Auth:JwtSigningKey"] = "integration-test-signing-key-at-least-32-bytes-long",
                ["Auth:BootstrapAdminUsername"] = TestAdminUsername,
                ["Auth:BootstrapAdminPassword"] = TestAdminPassword,
            });
        });
    }

    public Task InitializeAsync() => _container.StartAsync();

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await _container.DisposeAsync();
    }
}

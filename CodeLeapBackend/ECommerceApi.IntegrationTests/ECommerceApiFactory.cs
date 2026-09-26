using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;
using Xunit;

namespace ECommerceApi.IntegrationTests;

public sealed class ECommerceApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer database = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("codeleap_integration_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public async Task InitializeAsync()
    {
        await database.StartAsync();

        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", database.GetConnectionString());
        Environment.SetEnvironmentVariable("Jwt__Issuer", "CodeLeap.IntegrationTests");
        Environment.SetEnvironmentVariable("Jwt__Audience", "CodeLeap.IntegrationTests.Client");
        Environment.SetEnvironmentVariable("Jwt__Key", "integration-test-signing-key-that-is-long-enough-for-hmac-sha256");
        Environment.SetEnvironmentVariable("Jwt__AccessTokenExpirationMinutes", "60");
    }

    public new async Task DisposeAsync()
    {
        Dispose();
        await database.DisposeAsync();

        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", null);
        Environment.SetEnvironmentVariable("Jwt__Issuer", null);
        Environment.SetEnvironmentVariable("Jwt__Audience", null);
        Environment.SetEnvironmentVariable("Jwt__Key", null);
        Environment.SetEnvironmentVariable("Jwt__AccessTokenExpirationMinutes", null);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
    }
}

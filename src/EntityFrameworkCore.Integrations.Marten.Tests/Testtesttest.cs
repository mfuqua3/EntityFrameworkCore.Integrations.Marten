using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using TestingSandbox;

namespace EntityFrameworkCore.Integrations.Marten.Tests;

[TestFixture]
public class Testtesttest
{

    [Test]
    public async Task Setup()
    {
        var services = new ServiceCollection();
        var connectionString = "Server=localhost;Database=martenIntegrationDb;User Id=postgres;password=password;";
        services.AddDbContext<TestingSandboxDbContext>(x =>
        {
            x.UseNpgsql(connectionString);
            x.UseMartenIntegration(cfg =>
            {
                cfg.WithRegistry(new TestRegistry());
            });
        });
        var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        await using var context = scope.ServiceProvider.GetRequiredService<TestingSandboxDbContext>();

        //TODO Fix async, MartenQueryable<T> does not implement IAsyncEnumerable<T>
        var invoices =  context.Invoices.ToList();
        ;
    }
}
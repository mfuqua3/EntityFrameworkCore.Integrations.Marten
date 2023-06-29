using Marten;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;

namespace EntityFrameworkCore.Integrations.Marten.Tests;

[TestFixture]
public class Testtesttest
{

    [Test]
    public async Task Setup()
    {
        var services = new ServiceCollection();
        var connectionString = "Server=localhost;Database=martenIntegrationDb;User Id=postgres;password=password;";
        services.AddDbContext<TestDbContext>(x =>
        {
            x.UseNpgsql(connectionString);
            x.UseMartenIntegration(cfg =>
            {
                cfg.WithRegistry(new TestRegistry());
            });
        });
        var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        await using var context = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        var model = context.Model;
        //var migrator = context.GetService<IMigrator>();
    }
}

public class TestRegistry:MartenRegistry
{
    public TestRegistry()
    {
        For<Invoice>().Index(x => x.Amount);
    }
}

public class TestDbContext : MartenIntegratedDbContext
{
    public TestDbContext()
    {
        
    }

    public TestDbContext(DbContextOptions<TestDbContext> options):base(options)
    {
        
    }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbDocument<Invoice> Invoices { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public List<Order> Orders { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public string Content { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; }
}

public class Invoice
{
    public int Id { get; set; }
    public int Amount { get; set; }
    public int OrderId { get; set; }
}
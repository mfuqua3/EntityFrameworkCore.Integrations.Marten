using EntityFrameworkCore.Integrations.Marten;
using Microsoft.EntityFrameworkCore;

namespace TestingSandbox;

public class TestingSandboxDbContext : MartenIntegratedDbContext
{
    public TestingSandboxDbContext(DbContextOptions<TestingSandboxDbContext> options): base(options)
    {
        
    }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbDocument<Invoice> Invoices { get; set; }
}
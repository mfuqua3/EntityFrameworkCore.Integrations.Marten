using Marten;
using Microsoft.EntityFrameworkCore;

namespace TestingSandbox;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public void ConfigureServices(IServiceCollection services)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<TestingSandboxDbContext>(x =>
        {
            x.UseNpgsql(connectionString);
            x.UseMartenIntegration(store =>
            {
                store.WithRegistry(new TestRegistry());
            });
        });
    }
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, TestingSandboxDbContext dbContext)
    {
        dbContext.Database.Migrate();
    }
}
public class TestRegistry:MartenRegistry
{
    public TestRegistry()
    {
        For<Invoice>().Index(x=>x.Amount);
    }
}
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
            x.UseMartenIntegration();
        });
    }
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
    }
}
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EntityFrameworkCore.Integrations.Marten.Utilities;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection Replace<TService, TImplementation>(this IServiceCollection services,
        ServiceLifetime serviceLifetime = ServiceLifetime.Singleton)
        where TImplementation : TService
    {
        return services.Replace(new ServiceDescriptor(typeof(TService), typeof(TImplementation), serviceLifetime));
    }
}
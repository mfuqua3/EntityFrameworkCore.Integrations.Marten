using System.Globalization;
using EntityFrameworkCore.Integrations.Marten.Diagnostics;
using EntityFrameworkCore.Integrations.Marten.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace EntityFrameworkCore.Integrations.Marten.Infrastructure;

public class MartenIntegrationExtension : IDbContextOptionsExtension
{
    private readonly IServiceCollection _integrationServices = new ServiceCollection();
    private DbContextOptionsExtensionInfo? _info;

    public MartenIntegrationExtension()
    {
        ConfigureServices();
    }

    public MartenIntegrationExtension(MartenIntegrationExtension copyFrom)
    {
        _integrationServices = copyFrom._integrationServices;
    }

    private void ConfigureServices()
    {
        _integrationServices
            .AddSingleton<IDbDocumentFinder, DbDocumentFinder>()
            .AddSingleton<IDbDocumentInitializer, DbDocumentInitializer>()
            .AddSingleton<IDbDocumentSource, DbDocumentSource>();
    }

    public void ApplyServices(IServiceCollection services)
    {
        foreach (var serviceDescriptor in _integrationServices)
        {
            services.Add(serviceDescriptor);
        }
    }

    public void Validate(IDbContextOptions options)
    {
        //Validate that the registered DbContext is a MartenIntegratedDbContext
        var applicationServiceProvider = options.FindExtension<CoreOptionsExtension>()!.ApplicationServiceProvider;
        var contextOptions = applicationServiceProvider?.GetService<DbContextOptions>();
        if (contextOptions?.GetType().GetGenericArguments().FirstOrDefault() is { } contextType)
        {
            if (!contextType.IsAssignableTo(typeof(MartenIntegratedDbContext)))
            {
                throw new InvalidOperationException(MartenIntegrationStrings.InvalidContextType(contextType));
            }
        }
    }

    public DbContextOptionsExtensionInfo Info
        => _info ??= new ExtensionInfo(this);

    private sealed class ExtensionInfo : DbContextOptionsExtensionInfo
    {
        private int? _serviceProviderHash;

        public ExtensionInfo(IDbContextOptionsExtension extension) : base(extension)
        {
        }

        private new MartenIntegrationExtension Extension
            => (MartenIntegrationExtension)base.Extension;

        public override int GetServiceProviderHashCode()
        {
            if (_serviceProviderHash == null)
            {
                var hashCode = new HashCode();
                foreach (var serviceDescriptor in Extension._integrationServices)
                {
                    hashCode.Add(serviceDescriptor);
                }

                _serviceProviderHash = hashCode.ToHashCode();
            }

            return _serviceProviderHash.Value;
        }

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
            => true;

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
            debugInfo["MartenIntegration"] = true.GetHashCode().ToString(CultureInfo.InvariantCulture);
        }

        public override bool IsDatabaseProvider => false;

        public override string LogFragment => "MartenIntegrationEnabled ";
    }
}
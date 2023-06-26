using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using EntityFrameworkCore.Integrations.Marten.Internal;
using EntityFrameworkCore.Integrations.Marten.Utilities;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Weasel.Core;

namespace EntityFrameworkCore.Integrations.Marten.Infrastructure;

public class MartenIntegrationExtension : IDbContextOptionsExtension
{
    private readonly IServiceCollection _integrationServices = new ServiceCollection();
    private DbContextOptionsExtensionInfo? _info;
    private StoreOptions StoreOptions { get; set; } = new()
    {
        AutoCreateSchemaObjects = AutoCreate.None
    };
    public StoreOptions.PoliciesExpression Policies => StoreOptions.Policies;
    public int NameDataLength
    {
        get => StoreOptions.NameDataLength;
        set => StoreOptions.NameDataLength = value;
    }

    public string? SchemaName { get; set; }

    public MartenIntegrationExtension()
    {
        ConfigureServices();
    }

    public MartenIntegrationExtension(MartenIntegrationExtension copyFrom)
    {
        _integrationServices = copyFrom._integrationServices;
        StoreOptions = copyFrom.StoreOptions;
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

        services.AddMarten(StoreOptions);
    }

    [SuppressMessage("Usage", "EF1001:Internal EF Core API usage.")]
    public void Validate(IDbContextOptions options)
    {
        //Validate that the registered DbContext is a MartenIntegratedDbContext
        var coreOptions = options.FindExtension<CoreOptionsExtension>();
        var applicationServiceProvider = coreOptions!.ApplicationServiceProvider;
        var contextOptions = applicationServiceProvider?.GetService<DbContextOptions>();
        if (contextOptions?.GetType().GetGenericArguments().FirstOrDefault() is { } contextType)
        {
            if (!contextType.IsAssignableTo(typeof(MartenIntegratedDbContext)))
            {
                throw new InvalidOperationException(MartenIntegrationStrings.InvalidContextType(contextType));
            }
        }

        var npgSqlOptionsExtension = options.FindExtension<NpgsqlOptionsExtension>();
        if (npgSqlOptionsExtension == null)
        {
            throw new InvalidOperationException(MartenIntegrationStrings.InvalidProviderConfiguration());
        }

        StoreOptions.UpdateBatchSize = npgSqlOptionsExtension.MaxBatchSize ?? StoreOptions.UpdateBatchSize;
        SchemaName ??= coreOptions.Model?.GetDefaultSchema() ?? StoreOptions.DatabaseSchemaName;
        StoreOptions.DatabaseSchemaName = SchemaName;
        if (!string.IsNullOrEmpty(npgSqlOptionsExtension.ConnectionString))
        {
            StoreOptions.Connection(npgSqlOptionsExtension.ConnectionString);
        }
        else if (npgSqlOptionsExtension.Connection is NpgsqlConnection dbConnection)
        {
            StoreOptions.Connection(() => dbConnection);
        }
        else if (npgSqlOptionsExtension.DataSource != null)
        {
            StoreOptions.Connection(npgSqlOptionsExtension.DataSource.ConnectionString);
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
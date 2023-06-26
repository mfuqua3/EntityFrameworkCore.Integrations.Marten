using EntityFrameworkCore.Integrations.Marten.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
/// Provides extension methods on <see cref="DbContextOptionsBuilder"/> and <see cref="DbContextOptionsBuilder{T}"/>
/// used to configure a <see cref="DbContext"/> to context to a PostgreSQL database with Npgsql.
/// </summary>
public static class MartenIntegrationContextOptionsBuilderExtensions
{
    public static DbContextOptionsBuilder UseMartenIntegration(this DbContextOptionsBuilder optionsBuilder)
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder, nameof(optionsBuilder));
        var extension = GetOrCreateExtension(optionsBuilder);
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);
        return optionsBuilder;
    }

    private static MartenIntegrationExtension GetOrCreateExtension(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.Options.FindExtension<MartenIntegrationExtension>() is { } existing
            ? new MartenIntegrationExtension(existing)
            : new MartenIntegrationExtension();
}
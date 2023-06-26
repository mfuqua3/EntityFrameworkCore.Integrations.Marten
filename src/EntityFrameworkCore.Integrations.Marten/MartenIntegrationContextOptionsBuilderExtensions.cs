using EntityFrameworkCore.Integrations.Marten.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
/// Provides extension methods on <see cref="DbContextOptionsBuilder"/> and <see cref="DbContextOptionsBuilder{T}"/>
/// used to configure a <see cref="DbContext"/> to support integration with Marten.
/// </summary>
public static class MartenIntegrationContextOptionsBuilderExtensions
{
    public static DbContextOptionsBuilder UseMartenIntegration(this DbContextOptionsBuilder optionsBuilder,
        Action<MartenIntegratedDbContextOptionsBuilder>? martenOptionsAction = null)
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder, nameof(optionsBuilder));
        var extension = GetOrCreateExtension(optionsBuilder);
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);
        martenOptionsAction?.Invoke(new MartenIntegratedDbContextOptionsBuilder(optionsBuilder));
        return optionsBuilder;
    }

    private static MartenIntegrationExtension GetOrCreateExtension(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.Options.FindExtension<MartenIntegrationExtension>() is { } existing
            ? new MartenIntegrationExtension(existing)
            : new MartenIntegrationExtension();
}
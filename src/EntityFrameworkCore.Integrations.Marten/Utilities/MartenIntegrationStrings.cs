using EntityFrameworkCore.Integrations.Marten.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace EntityFrameworkCore.Integrations.Marten.Utilities;

internal static class MartenIntegrationStrings
{
    public static string InvalidContextType(Type contextType)
        => string.Format(
            ExceptionStrings.InvalidContextType,
            contextType.Name,
            nameof(MartenIntegratedDbContext),
            nameof(MartenIntegrationExtension));
    
    public static string InvalidProviderConfiguration()
        => string.Format(
            ExceptionStrings.InvalidProviderConfiguration,
            nameof(MartenIntegrationExtension),
            nameof(NpgsqlOptionsExtension));

    public static string DocumentInitializationFailed(Type entityType)
        => string.Format(
            ExceptionStrings.DocumentInitializationFailed,
            typeof(DbDocument<>).Name,
            entityType.Name);
    public static string CannotResolveMartenResource(Type resourceType)
        => string.Format(
            ExceptionStrings.CannotResolveMartenResource,
            resourceType.Name);

}
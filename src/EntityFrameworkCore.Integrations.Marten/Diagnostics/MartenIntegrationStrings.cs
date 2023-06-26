using System.Resources;
using EntityFrameworkCore.Integrations.Marten.Infrastructure;

namespace EntityFrameworkCore.Integrations.Marten.Diagnostics;

internal static class MartenIntegrationStrings
{
    public static string InvalidContextType(Type contextType)
        => string.Format(
            ExceptionStrings.InvalidContextType,
            contextType.Name,
            nameof(MartenIntegratedDbContext),
            nameof(MartenIntegrationExtension));
}
using EntityFrameworkCore.Integrations.Marten.Exceptions;

namespace EntityFrameworkCore.Integrations.Marten.Utilities;

internal static class Assert
{
    public static void MartenResolvedForEntity<TEntity, TService>(TService? martenService)
    {
        if (martenService == null)
        {
            throw new DbDocumentInitializationException(
                string.Join(". ",
                    MartenIntegrationStrings.DocumentInitializationFailed(typeof(TEntity)),
                    MartenIntegrationStrings.CannotResolveMartenResource(typeof(TService))));
        }
    }
}
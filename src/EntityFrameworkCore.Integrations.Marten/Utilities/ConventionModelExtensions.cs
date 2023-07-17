using Microsoft.EntityFrameworkCore.Metadata;

namespace EntityFrameworkCore.Integrations.Marten.Utilities;

internal static class ConventionModelExtensions
{
    public static IEnumerable<IConventionEntityType> GetMartenManagedEntities(this IConventionModel model)
        => model.GetEntityTypes()
            .Where(x => x.FindAnnotation("EntityManagement")?.Value?.ToString() == "Marten");
}
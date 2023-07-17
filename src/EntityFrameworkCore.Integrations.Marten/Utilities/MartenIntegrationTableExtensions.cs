using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EntityFrameworkCore.Integrations.Marten.Utilities;

public static class MartenIntegrationTableExtensions
{
    public static bool IsMartenGenerated(this IAnnotatable table)
        => table.FindAnnotation(MartenIntegrationAnnotationNames.EntityManagement)?.Value
            ?.Equals(MartenIntegrationAnnotationValues.MartenEntityManagement) ?? false;
}
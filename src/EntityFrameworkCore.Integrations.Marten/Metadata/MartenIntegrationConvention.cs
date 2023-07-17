using EntityFrameworkCore.Integrations.Marten.Utilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace EntityFrameworkCore.Integrations.Marten.Metadata;

public class MartenIntegrationConvention : IModelInitializedConvention
{
    public void ProcessModelInitialized(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
    {
        var assemblyVersion = GetType().Assembly.GetName().Version?.ToString() ?? "prerelease";
        modelBuilder.HasAnnotation(MartenIntegrationAnnotationNames.MartenIntegration, assemblyVersion, true);
    }
}
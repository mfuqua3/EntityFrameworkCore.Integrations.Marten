using EntityFrameworkCore.Integrations.Marten.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace EntityFrameworkCore.Integrations.Marten.Infrastructure;

public class MartenIntegrationConventionSetPlugin : IConventionSetPlugin
{
    private readonly MartenIntegrationConventionSetBuilderDependencies _dependencies;

    public MartenIntegrationConventionSetPlugin(MartenIntegrationConventionSetBuilderDependencies dependencies)
    {
        _dependencies = dependencies;
    }

    public ConventionSet ModifyConventions(ConventionSet conventionSet)
    {
        conventionSet.ModelInitializedConventions.Add(new MartenIntegrationConvention());
        conventionSet.ModelInitializedConventions.Add(new DbDocumentFindingConvention(_dependencies));
        conventionSet.ModelInitializedConventions.Add(new MartenStorageModelConvention(_dependencies));
        return conventionSet;
    }
}
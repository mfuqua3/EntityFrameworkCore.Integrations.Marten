﻿using EntityFrameworkCore.Integrations.Marten.Utilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace EntityFrameworkCore.Integrations.Marten.Metadata;

public class DbDocumentFindingConvention : IModelInitializedConvention
{
    public DbDocumentFindingConvention(MartenIntegrationConventionSetBuilderDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    protected virtual MartenIntegrationConventionSetBuilderDependencies Dependencies { get; }

    public void ProcessModelInitialized(IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var documentInfo in Dependencies.DocumentFinder.FindDocuments(Dependencies.ContextType))
        {
            modelBuilder.MartenManagedEntity(documentInfo.Type);
        }
    }
}
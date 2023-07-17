using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EntityFrameworkCore.Integrations.Marten.Utilities;

internal static class ConventionModelBuilderExtensions
{
    public static IConventionModelBuilder MartenManagedEntity(this IConventionModelBuilder modelBuilder, Type type)
        => modelBuilder.Entity(type, fromDataAnnotation: true)!
            .HasAnnotation("EntityManagement", "Marten")!
            .ModelBuilder;
}
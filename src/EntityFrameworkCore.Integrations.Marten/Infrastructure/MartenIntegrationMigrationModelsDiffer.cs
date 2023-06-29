using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update.Internal;

namespace EntityFrameworkCore.Integrations.Marten.Infrastructure;

[SuppressMessage("Usage", "EF1001:Internal EF Core API usage.")]
public class MartenIntegrationMigrationModelsDiffer : MigrationsModelDiffer
{
    public MartenIntegrationMigrationModelsDiffer(IRelationalTypeMappingSource typeMappingSource,
        IMigrationsAnnotationProvider migrationsAnnotationProvider, IRowIdentityMapFactory rowIdentityMapFactory,
        CommandBatchPreparerDependencies commandBatchPreparerDependencies) : base(typeMappingSource,
        migrationsAnnotationProvider, rowIdentityMapFactory, commandBatchPreparerDependencies)
    {
    }

    protected override IEnumerable<MigrationOperation> Diff(
        ITable source,
        ITable target,
        DiffContext diffContext)
    {
        if (source.IsMartenGenerated() && target.IsMartenGenerated())
        {
            yield break;
        }

        if (source.IsMartenGenerated() ^ target.IsMartenGenerated())
        {
            foreach (var operation in Remove(source, diffContext))
            {
                yield return operation;
            }
            foreach (var operation in Add(target, diffContext))
            {
                yield return operation;
            }

            yield break;
        }

        base.Diff(source, target, diffContext);
    }

    protected override IEnumerable<MigrationOperation> Add(ITable target, DiffContext diffContext)
    {
        return base.Add(target, diffContext);
    }

    protected override IEnumerable<MigrationOperation> Remove(ITable source, DiffContext diffContext)
    {
        return base.Remove(source, diffContext);
    }
}


internal static class TableExtensions
{
    public static bool IsMartenGenerated(this ITable table)
        => Equals(table.FindAnnotation("EntityGenerationStrategy")?.Value, "Marten");
}
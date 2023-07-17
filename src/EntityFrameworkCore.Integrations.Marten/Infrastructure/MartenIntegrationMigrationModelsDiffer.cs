using System.Diagnostics.CodeAnalysis;
using EntityFrameworkCore.Integrations.Marten.Design;
using EntityFrameworkCore.Integrations.Marten.Utilities;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Newtonsoft.Json;

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

    protected override IEnumerable<MigrationOperation> Diff(IRelationalModel? source, IRelationalModel? target,
        DiffContext diffContext)
    {
        foreach (var operation in base.Diff(source, target, diffContext))
            yield return operation;
        if (target != null && target.Tables.Any(x => x.IsMartenGenerated()) &&
            (source == null || source.Tables.All(x => !x.IsMartenGenerated())))
        {
            yield return new CreateMartenSystemFunctionsOperation();
        }
    }

    protected override IEnumerable<MigrationOperation> Diff(ITable source, ITable target, DiffContext diffContext)
    {
        if (source.IsMartenGenerated() && target.IsMartenGenerated())
        {
            var sourceColumns = source.Columns.ToArray();
            var targetColumns = target.Columns.ToArray();
            var functionsRequireUpdate = !target.SchemaQualifiedName.Equals(source.SchemaQualifiedName) ||
                                         sourceColumns.Length != targetColumns.Length || !targetColumns.All(tc =>
                                             sourceColumns.Any(sc =>
                                                 sc.Name == tc.Name && sc.StoreType == tc.StoreType));
            if (functionsRequireUpdate)
            {
                yield return new DropMartenTableFunctionsOperation
                {
                    SchemaQualifiedTableName = target.SchemaQualifiedName
                };
                yield return new CreateMartenTableFunctionsOperation
                    { SchemaQualifiedTableName = target.SchemaQualifiedName };
            }
        }

        foreach (var baseOperation in base.Diff(source, target, diffContext))
        {
            yield return baseOperation;
        }
    }

    protected override IEnumerable<MigrationOperation> Add(ITable target, DiffContext diffContext)
    {
        foreach (var baseOperation in base.Add(target, diffContext))
        {
            yield return baseOperation;
        }

        if (target.IsMartenGenerated())
        {
            yield return new CreateMartenTableFunctionsOperation
                { SchemaQualifiedTableName = target.SchemaQualifiedName };
        }
    }

    protected override IEnumerable<MigrationOperation> Remove(ITable source, DiffContext diffContext)
    {
        foreach (var baseOperation in base.Remove(source, diffContext))
        {
            yield return baseOperation;
        }

        if (source.IsMartenGenerated())
        {
            yield return new DropMartenTableFunctionsOperation
                { SchemaQualifiedTableName = source.SchemaQualifiedName };
        }
    }

    protected override IEnumerable<MigrationOperation> Add(ITableIndex target, DiffContext diffContext)
    {
        return target.IsMartenComputedIndex()
            ? new[] { CreateComputedIndexOperation.CreateFrom(target) }
            : base.Add(target, diffContext);
    }
}
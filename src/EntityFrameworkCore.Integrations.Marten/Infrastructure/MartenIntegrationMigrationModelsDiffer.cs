using System.Diagnostics.CodeAnalysis;
using EntityFrameworkCore.Integrations.Marten.Design;
using EntityFrameworkCore.Integrations.Marten.Metadata;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Newtonsoft.Json;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations.Internal;

namespace EntityFrameworkCore.Integrations.Marten.Infrastructure;

public class MartenIntegrationHistoryRepository : NpgsqlHistoryRepository
{
    private IModel? _model;
    public MartenIntegrationHistoryRepository(HistoryRepositoryDependencies dependencies) : base(dependencies)
    {
    }

    public override string GetCreateScript()
    {
        var model = EnsureModel();

        var operations = Dependencies.ModelDiffer.GetDifferences(null, model.GetRelationalModel());
        var commandList = Dependencies.MigrationsSqlGenerator.Generate(operations, model);

        return string.Concat(commandList.Select(c => c.CommandText));
    }
    
    private IModel EnsureModel()
    {
        if (_model == null)
        {
            var conventionSet = Dependencies.ConventionSetBuilder.CreateConventionSet();

            conventionSet.Remove(typeof(DbDocumentFindingConvention));
            conventionSet.Remove(typeof(DbSetFindingConvention));
            conventionSet.Remove(typeof(RelationalDbFunctionAttributeConvention));

            var modelBuilder = new ModelBuilder(conventionSet);
            modelBuilder.Entity<HistoryRow>(
                x =>
                {
                    ConfigureTable(x);
                    x.ToTable(TableName, TableSchema);
                });

            _model = Dependencies.ModelRuntimeInitializer.Initialize(
                (IModel)modelBuilder.Model, designTime: true, validationLogger: null);
        }

        return _model;
    }
}

[SuppressMessage("Usage", "EF1001:Internal EF Core API usage.")]
public class MartenIntegrationMigrationModelsDiffer : MigrationsModelDiffer
{
    public MartenIntegrationMigrationModelsDiffer(IRelationalTypeMappingSource typeMappingSource,
        IMigrationsAnnotationProvider migrationsAnnotationProvider, IRowIdentityMapFactory rowIdentityMapFactory,
        CommandBatchPreparerDependencies commandBatchPreparerDependencies) : base(typeMappingSource,
        migrationsAnnotationProvider, rowIdentityMapFactory, commandBatchPreparerDependencies)
    {
    }

    protected override IEnumerable<MigrationOperation> Diff(IEnumerable<ITable> source, IEnumerable<ITable> target, DiffContext diffContext)
    {
        return base.Diff(source, target, diffContext);
    }

    protected override IEnumerable<MigrationOperation> Add(ITableIndex target, DiffContext diffContext)
    {
        return target.IsMartenComputedIndex() ? new[] { CreateComputedIndexOperation.CreateFrom(target) } : base.Add(target, diffContext);
    }
}

internal static class TableExtensions
{
    public static bool IsMartenGenerated(this ITable table)
        => Equals(table.FindAnnotation("EntityGenerationStrategy")?.Value, "Marten");
}

internal static class TableIndexExtensions
{
    public static bool IsMartenComputedIndex(this ITableIndex table)
        => Equals(table.FindAnnotation("MartenIndexType")?.Value, "ComputedIndex");
}
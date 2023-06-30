using System.Diagnostics.CodeAnalysis;
using EntityFrameworkCore.Integrations.Marten.Design;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations;
using Weasel.Core;
using Weasel.Postgresql.Tables;

namespace EntityFrameworkCore.Integrations.Marten.Infrastructure;

public class MartenIntegrationMigrationSqlGenerator : NpgsqlMigrationsSqlGenerator
{
    [SuppressMessage("Usage", "EF1001:Internal EF Core API usage.")]
    public MartenIntegrationMigrationSqlGenerator(MigrationsSqlGeneratorDependencies dependencies,
        INpgsqlSingletonOptions npgsqlSingletonOptions) : base(dependencies, npgsqlSingletonOptions)
    {
    }

    protected override void Generate(MigrationOperation operation, IModel? model, MigrationCommandListBuilder builder)
    {
        if (operation is CreateComputedIndexOperation computedIndexOperation)
        {
            GenerateCreateComputedIndex(computedIndexOperation, builder);
            return;
        }
        base.Generate(operation, model, builder);
    }

    private void GenerateCreateComputedIndex(CreateComputedIndexOperation computedIndexOperation, MigrationCommandListBuilder builder)
    {
        var martenComputedIndex = new IndexDefinition(computedIndexOperation.Name)
        {
            Method = (IndexMethod)computedIndexOperation.Method,
            SortOrder = (SortOrder)computedIndexOperation.SortOrder,
            NullsSortOrder = (NullsSortOrder)computedIndexOperation.NullsSortOrder,
            IsUnique = computedIndexOperation.IsUnique,
            IsConcurrent = computedIndexOperation.IsConcurrent,
            Columns = computedIndexOperation.Columns,
            IncludeColumns = computedIndexOperation.IncludeColumns,
            Mask = computedIndexOperation.Mask,
            TableSpace = computedIndexOperation.TableSpace,
            Predicate = computedIndexOperation.Predicate,
            Collation = computedIndexOperation.Collation,
        };
        if (computedIndexOperation.FillFactor.HasValue)
        {
            martenComputedIndex.FillFactor = computedIndexOperation.FillFactor;
        }
        if (computedIndexOperation.CustomMethod != null)
        {
            martenComputedIndex.CustomMethod = computedIndexOperation.CustomMethod;
        }

        builder.AppendLine(martenComputedIndex.ToDDL(new Table(computedIndexOperation.Table)));
        builder.EndCommand();
    }
}
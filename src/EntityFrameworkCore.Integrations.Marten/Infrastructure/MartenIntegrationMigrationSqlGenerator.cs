using System.Diagnostics.CodeAnalysis;
using System.Text;
using EntityFrameworkCore.Integrations.Marten.Design;
using EntityFrameworkCore.Integrations.Marten.Exceptions;
using EntityFrameworkCore.Integrations.Marten.Metadata;
using EntityFrameworkCore.Integrations.Marten.Utilities;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations;
using Weasel.Core;
using Weasel.Core.Migrations;
using Weasel.Postgresql.Functions;
using Weasel.Postgresql.Tables;

namespace EntityFrameworkCore.Integrations.Marten.Infrastructure;

public class MartenIntegrationMigrationSqlGenerator : NpgsqlMigrationsSqlGenerator
{
    private readonly IMartenIntegrationSingletonOptions _martenIntegrationOptions;

    [SuppressMessage("Usage", "EF1001:Internal EF Core API usage.")]
    public MartenIntegrationMigrationSqlGenerator(MigrationsSqlGeneratorDependencies dependencies,
        INpgsqlSingletonOptions npgsqlSingletonOptions, IMartenIntegrationSingletonOptions martenIntegrationOptions) :
        base(dependencies, npgsqlSingletonOptions)
    {
        _martenIntegrationOptions = martenIntegrationOptions;
    }

    protected override void Generate(MigrationOperation operation, IModel? model, MigrationCommandListBuilder builder)
    {
        if (operation is CreateComputedIndexOperation computedIndexOperation)
        {
            GenerateCreateComputedIndex(computedIndexOperation, builder);
            return;
        }

        if (operation is MartenTableFunctionsOperation martenTableFunctionsOperation)
        {
            GenerateForMartenTableFunctions(martenTableFunctionsOperation, builder);
            return;
        }

        base.Generate(operation, model, builder);
    }

    private void GenerateForMartenTableFunctions(MartenTableFunctionsOperation martenTableFunctionsOperation,
        MigrationCommandListBuilder builder)
    {
        var table = Dependencies.CurrentContext.Context.Model.GetEntityTypes().SingleOrDefault(x =>
            x.GetSchemaQualifiedTableName() == martenTableFunctionsOperation.SchemaQualifiedTableName);
        //TODO THIS WONT WORK FOR DROPS
        if (table == null)
        {
            throw new Exception($"Entity for {martenTableFunctionsOperation.SchemaQualifiedTableName} not found");
        }

        var tableFeature = _martenIntegrationOptions.StoreOptions.Storage.FindFeature(table.ClrType);
        if (tableFeature == null)
        {
            throw new MartenIntegrationException(
                MartenIntegrationStrings.MartenResourceNotFound(
                    $"{nameof(IFeatureSchema)}:{table.ClrType.Name}"));
        }

        var functions = tableFeature.Objects.Where(x => x is Function).Cast<Function>();
        var isDrop = martenTableFunctionsOperation is DropMartenTableFunctionsOperation;
        foreach (var function in functions)
        {
            if (isDrop)
            {
                GenerateDropMartenFunction(function, tableFeature.Migrator, builder);
            }
            else
            {
                GenerateUpdateMartenFunction(function, tableFeature.Migrator, builder);
            }
        }

        builder.EndCommand();
    }

    private void GenerateUpdateMartenFunction(Function function ,Migrator migrator,
        MigrationCommandListBuilder builder)
    {
        var sqlSb = new StringBuilder();
        using var stringWriter = new StringWriter(sqlSb);
        function.WriteCreateStatement(migrator, stringWriter);
        builder.AppendLine(sqlSb.ToString());
    }

    private void GenerateDropMartenFunction(Function function,Migrator migrator,
        MigrationCommandListBuilder builder)
    {
        var sqlSb = new StringBuilder();
        using var stringWriter = new StringWriter(sqlSb);
        function.WriteDropStatement(migrator, stringWriter);
        builder.AppendLine(sqlSb.ToString());
    }

    private void GenerateCreateComputedIndex(CreateComputedIndexOperation computedIndexOperation,
        MigrationCommandListBuilder builder)
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
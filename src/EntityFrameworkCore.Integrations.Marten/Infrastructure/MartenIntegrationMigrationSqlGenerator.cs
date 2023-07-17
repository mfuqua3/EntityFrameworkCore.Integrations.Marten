using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using EntityFrameworkCore.Integrations.Marten.Design;
using EntityFrameworkCore.Integrations.Marten.Exceptions;
using EntityFrameworkCore.Integrations.Marten.Metadata;
using EntityFrameworkCore.Integrations.Marten.Utilities;
using Marten.Schema.Identity.Sequences;
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

        if (operation is CreateMartenSystemFunctionsOperation createMartenSystemFunctionsOperation)
        {
            GenerateCreateMartenSystemFunctions(createMartenSystemFunctionsOperation, builder);
            return;
        }

        base.Generate(operation, model, builder);
    }

    private void GenerateCreateMartenSystemFunctions(CreateMartenSystemFunctionsOperation _,
        MigrationCommandListBuilder builder)
    {
        var martenStorage = _martenIntegrationOptions.StoreOptions.Storage;
        var systemFunctions = martenStorage.GetType()
            .GetProperty("SystemFunctions", BindingFlags.Instance | BindingFlags.NonPublic)?
            .GetValue(martenStorage) as IFeatureSchema;
        if (systemFunctions == null)
        {
            throw new MartenIntegrationException(
                MartenIntegrationStrings.MartenResourceNotFound(
                    $"{nameof(IFeatureSchema)}:SystemFunctions"));
        }

        foreach (var schemaObject in systemFunctions.Objects)
        {
            var sqlSb = new StringBuilder();
            var writer = new StringWriter(sqlSb);
            schemaObject.WriteCreateStatement(systemFunctions.Migrator, writer);
            builder.AppendLine(sqlSb.ToString());
        }

        GenerateHiloInfrastructure(systemFunctions.Migrator.DefaultSchemaName, builder);
        builder.EndCommand();
    }

    private void GenerateHiloInfrastructure(string schema, MigrationCommandListBuilder builder)
    {
        builder.AppendLine(
            $@"create table {schema}.mt_hilo
(
    entity_name varchar not null
        constraint pkey_mt_hilo_entity_name
            primary key,
    hi_value    bigint default 0
);");
        
        builder.AppendLine(
            $@"create or replace function mt_get_next_hi(entity character varying) returns integer
    language plpgsql
as
$$
DECLARE
current_value bigint;
    next_value
bigint;
BEGIN
select hi_value
into current_value
from {schema}.mt_hilo
where entity_name = entity;
IF
current_value is null THEN
        insert into {schema}.mt_hilo (entity_name, hi_value) values (entity, 0);
        next_value
:= 0;
ELSE
        next_value := current_value + 1;
update {schema}.mt_hilo
set hi_value = next_value
where entity_name = entity and hi_value = current_value;

IF
NOT FOUND THEN
            next_value := -1;
END IF;
END IF;

return next_value;
END

$$;");
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
                GenerateCreateMartenFunction(function, tableFeature.Migrator, builder);
            }
        }

        builder.EndCommand();
    }

    private void GenerateCreateMartenFunction(Function function, Migrator migrator,
        MigrationCommandListBuilder builder)
    {
        var sqlSb = new StringBuilder();
        using var stringWriter = new StringWriter(sqlSb);
        function.WriteCreateStatement(migrator, stringWriter);
        builder.AppendLine(sqlSb.ToString());
    }

    private void GenerateDropMartenFunction(Function function, Migrator migrator,
        MigrationCommandListBuilder builder)
    {
        builder.AppendLine($"DROP FUNCTION IF EXISTS {function.Identifier.QualifiedName};");
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
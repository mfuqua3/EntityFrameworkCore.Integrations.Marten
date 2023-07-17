using System.Data;
using System.Reflection;
using EntityFrameworkCore.Integrations.Marten.Exceptions;
using EntityFrameworkCore.Integrations.Marten.Utilities;
using Marten.Schema;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using NpgsqlTypes;
using Weasel.Core.Migrations;
using Weasel.Postgresql;
using Weasel.Postgresql.Functions;
using Weasel.Postgresql.Tables;
using Sequence = Weasel.Postgresql.Sequence;
using SortOrder = Weasel.Postgresql.Tables.SortOrder;
using Table = Weasel.Postgresql.Tables.Table;
using View = Weasel.Postgresql.Views.View;

namespace EntityFrameworkCore.Integrations.Marten.Metadata;

public class MartenStorageModelConvention : IModelInitializedConvention
{
    public MartenStorageModelConvention(MartenIntegrationConventionSetBuilderDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    protected virtual MartenIntegrationConventionSetBuilderDependencies Dependencies { get; }

    public void ProcessModelInitialized(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        var martenManagedEntities = modelBuilder.Metadata.GetMartenManagedEntities();
        foreach (var entityType in martenManagedEntities)
        {
            var feature = Dependencies.StoreOptions.Storage.FindFeature(entityType.ClrType);
            try
            {
                ProcessFeature(modelBuilder, feature, entityType);
            }
            catch (Exception ex)
            {
                throw new MartenEntityBuilderException(
                    MartenIntegrationStrings.EntityBuilderFailure("Entity", entityType.Name), ex);
            }
        }
    }

    private void ProcessFeature(IConventionModelBuilder modelBuilder, IFeatureSchema featureSchema,
        IConventionEntityType entityType)
    {
        var documentType = entityType.ClrType;
        var schemaObjects = featureSchema.Objects;
        foreach (var schemaObject in schemaObjects)
        {
            switch (schemaObject)
            {
                case Table table:
                    TryExecuteModelOperation(
                        () => ProcessTable(modelBuilder, table, entityType),
                        nameof(Table),
                        table.Identifier.QualifiedName);
                    break;
                case Function function:
                    TryExecuteModelOperation(
                        () => ProcessFunction(modelBuilder, function, entityType),
                        nameof(Function),
                        function.Identifier.QualifiedName);
                    break;
                case Sequence sequence:
                    TryExecuteModelOperation(
                        () => ProcessSequence(modelBuilder, sequence, documentType),
                        nameof(Sequence),
                        sequence.Identifier.QualifiedName);
                    break;
                case View view:
                    TryExecuteModelOperation(
                        () => ProcessView(modelBuilder, view, documentType),
                        nameof(View),
                        view.Identifier.QualifiedName);
                    break;
            }
        }
    }

    private void ProcessTable(IConventionModelBuilder modelBuilder, Table table, IConventionEntityType entityType)
    {
        var propertyBuilderLookup = new Dictionary<string, IConventionPropertyBuilder>();
        var entityTypeBuilder = entityType.Builder;
        entityTypeBuilder.ToTable(table.Identifier.Name, table.Identifier.Schema);

        var dataColumn = table.Columns.FirstOrDefault(x =>
            x.Name.Contains("data", StringComparison.OrdinalIgnoreCase) && string.Equals(x.Type,
                NpgsqlDbType.Jsonb.ToString(), StringComparison.OrdinalIgnoreCase))!;
        foreach (var entityProperty in entityTypeBuilder.Metadata.ClrType.GetProperties())
        {
            var primaryKeyMatch = table.PrimaryKeyColumns.FirstOrDefault(x =>
                string.Equals(x, entityProperty.Name, StringComparison.OrdinalIgnoreCase));
            if (primaryKeyMatch != null)
            {
                var propertyBuilder = entityTypeBuilder.Property(entityProperty)!;
                propertyBuilder.HasColumnName(primaryKeyMatch)!
                    .HasValueGenerationStrategy(NpgsqlValueGenerationStrategy.None);
                continue;
            }

            entityTypeBuilder.Ignore(entityProperty.Name);
        }

        foreach (var column in table.Columns)
        {
            var type = GetClrType(column.Type);
            var propertyBuilder = entityTypeBuilder.Property(type, column.Name)!
                .HasColumnType(column.Type)!
                .HasColumnName(column.Name)!;
            propertyBuilderLookup.Add(column.Name, propertyBuilder);
        }

        if (dataColumn == null)
        {
            throw new InvalidOperationException("Unable to determine the name of the Marten data column");
        }


        foreach (var index in table.Indexes)
        {
            var ddl = index.ToDDL(table);
            TryExecuteModelOperation(
                () => ProcessIndex(entityTypeBuilder, index),
                "Index",
                index.Name);
        }

        entityTypeBuilder.PrimaryKey(table.PrimaryKeyColumns.Select(x => propertyBuilderLookup[x].Metadata).ToArray())!
            .HasName(table.PrimaryKeyName);
        entityTypeBuilder.HasAnnotation("EntityGenerationStrategy", "Marten");
    }

    private void ProcessIndex(IConventionEntityTypeBuilder entityBuilder, IndexDefinition index)
    {
        if (index.Columns == null || index.Columns.Length == 0)
        {
            throw new MartenEntityBuilderException(
                MartenIntegrationStrings.EntityBuilderFailure("Index", index.Name,
                    MartenIntegrationStrings.MartenResourceNotFound(nameof(index.Columns))));
        }

        if (index is ComputedIndex computedIndex)
        {
            ProcessIndex(entityBuilder, computedIndex);
            return;
        }

        var columns = index.Columns;
        var indexBuilder = entityBuilder.HasIndex(columns, index.Name)!
            .IsUnique(index.IsUnique)!
            .HasMethod(index.Method.ToString())!
            .IsCreatedConcurrently(index.IsConcurrent)!
            .IsDescending(index.Columns.Select(_ => index.SortOrder == SortOrder.Desc).ToArray())!
            .HasNullSortOrder(index.Columns.Select(_ =>
            {
                return index.NullsSortOrder switch
                {
                    NullsSortOrder.First => NullSortOrder.NullsFirst,
                    NullsSortOrder.Last => NullSortOrder.NullsLast,
                    _ => NullSortOrder.Unspecified
                };
            }).ToArray(), false)!;
        if (!string.IsNullOrEmpty(index.Collation))
        {
            indexBuilder.UseCollation(index.Columns.Select(_ => index.Collation).ToArray(), false);
        }

        if (!string.IsNullOrEmpty(index.Predicate))
        {
            indexBuilder.HasFilter(index.Predicate);
        }
    }

    private void ProcessIndex(IConventionEntityTypeBuilder entityBuilder, ComputedIndex index)
    {
        var columns = index.Columns.Select(c =>
            {
                var jsonMatch = MartenIntegrationPatterns.JsonBColumnNameExtraction.Match(c);
                return jsonMatch.Success ? jsonMatch.Groups[1].Value : c;
            }).Distinct()
            .ToArray();
        var indexBuilder = entityBuilder.HasIndex(columns, index.Name)!
            .HasAnnotation("MartenIndexType", "ComputedIndex", true)!;
        var indexProperties = index.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(pi =>
            pi.PropertyType.IsPrimitive ||
            pi.PropertyType.IsEnum ||
            pi.PropertyType == typeof(string) ||
            pi.PropertyType == typeof(string[]));
        foreach (var indexProperty in indexProperties)
        {
            var value = indexProperty.GetValue(index);
            if (indexProperty.PropertyType.IsEnum)
            {
                value = Convert.ToInt32(value);
            }

            indexBuilder.HasAnnotation($"MartenComputedIndex:{indexProperty.Name}", value, true);
        }
    }

    private void TryExecuteModelOperation(Action operation, string modelElementType, string modelElementName)
    {
        try
        {
            operation.Invoke();
        }
        catch (Exception ex)
        {
            throw new MartenEntityBuilderException(
                MartenIntegrationStrings.EntityBuilderFailure(modelElementType, modelElementName), ex);
        }
    }

    private Type GetClrType(string columnType)
    {
        var typeMappings = NpgsqlTypeMapper.Mappings;
        var mapping = typeMappings.SingleOrDefault(x => x.DataTypeName == columnType);
        mapping ??= typeMappings.SingleOrDefault(x =>
            string.Equals(x.NpgsqlDbType?.ToString(), columnType, StringComparison.CurrentCultureIgnoreCase));
        if (mapping == null)
        {
            throw new ArgumentException(
                MartenIntegrationStrings.NoNpgsqlTypeMapping(columnType),
                nameof(columnType));
        }

        var clrType = mapping.ClrTypes.FirstOrDefault() ??
                      Type.GetType(string.Join('.', "System", mapping.DbType.ToString()));
        if (clrType == null)
        {
            throw new ArgumentException(
                MartenIntegrationStrings.NoNpgsqlTypeMapping(columnType),
                nameof(columnType));
        }

        return clrType;
    }

    private string GetDbType(Type clrType)
    {
        var typeMappings = NpgsqlTypeMapper.Mappings;
        var mapping = typeMappings.FirstOrDefault(x => x.ClrTypes.Contains(clrType));
        if (mapping?.DataTypeName == null)
        {
            throw new ArgumentException(
                MartenIntegrationStrings.NoNpgsqlTypeMapping(clrType.Name),
                nameof(clrType));
        }

        return mapping.DataTypeName;
    }


    private void ProcessFunction(IConventionModelBuilder modelBuilder, Function function, IConventionEntityType entityType)
    {
        function.
        entityType.Builder
            .HasAnnotation("schema_function_" + function.Identifier.Name, function.Body().GetHashCode());
    }

    private void ProcessSequence(IConventionModelBuilder modelBuilder, Sequence function, Type documentType)
    {
    }

    private void ProcessView(IConventionModelBuilder modelBuilder, View view, Type documentType)
    {
    }
}
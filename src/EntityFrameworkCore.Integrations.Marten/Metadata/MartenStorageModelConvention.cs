using System.Data.Common;
using System.Reflection;
using System.Text;
using EntityFrameworkCore.Integrations.Marten.Exceptions;
using EntityFrameworkCore.Integrations.Marten.Utilities;
using Marten.Schema;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using NpgsqlTypes;
using Weasel.Core;
using Weasel.Core.Migrations;
using Weasel.Postgresql;
using Weasel.Postgresql.Tables;
using Sequence = Weasel.Postgresql.Sequence;
using SortOrder = Weasel.Postgresql.Tables.SortOrder;
using Table = Weasel.Postgresql.Tables.Table;

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
        var schemaObjects = featureSchema.Objects.OrderBy(x => x is Table);
        foreach (var schemaObject in schemaObjects)
        {
            switch (schemaObject)
            {
                case Table table:
                    TryExecuteModelOperation(
                        () => ProcessTable(table, entityType),
                        nameof(Table),
                        table.Identifier.QualifiedName);
                    break;
                case Sequence sequence:
                    TryExecuteModelOperation(
                        () => ProcessSequence(modelBuilder, sequence, entityType),
                        nameof(Sequence),
                        sequence.Identifier.QualifiedName);
                    break;
            }
        }
    }

    private void ProcessTable(Table table, IConventionEntityType entityType)
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
            TryExecuteModelOperation(
                () => ProcessIndex(entityTypeBuilder, index),
                "Index",
                index.Name);
        }

        entityTypeBuilder.PrimaryKey(table.PrimaryKeyColumns.Select(x => propertyBuilderLookup[x].Metadata).ToArray())!
            .HasName(table.PrimaryKeyName);
        entityTypeBuilder.HasAnnotation(MartenIntegrationAnnotationNames.EntityManagement,
            MartenIntegrationAnnotationValues.MartenEntityManagement, true);
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
            .HasAnnotation(MartenIntegrationAnnotationNames.ComputedIndex.Type, "ComputedIndex", true)!;
        var indexProperties = index.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(pi =>
            pi.PropertyType.IsPrimitive ||
            pi.PropertyType.IsEnum ||
            pi.PropertyType == typeof(string) ||
            pi.PropertyType == typeof(string[]));
        foreach (var indexProperty in indexProperties)
        {
            if (indexProperty.Name == nameof(ComputedIndex.Name))
            {
                continue;
            }
            var value = indexProperty.GetValue(index);
            if (indexProperty.PropertyType.IsEnum)
            {
                value = Convert.ToInt32(value);
            }

            if (value.IsNullOrDefault())
            {
                continue;
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

    private void ProcessSequence(IConventionModelBuilder modelBuilder, Sequence sequence,
        IConventionEntityType entityType)
    {
        var insertStatementSb = new StringBuilder();
        using var writer = new StringWriter(insertStatementSb);
        var migrator = new NullMigrator();
        sequence.WriteCreateStatement(migrator, writer);
        var insertStatement = insertStatementSb.ToString();
        var sequenceStartMatch = MartenIntegrationPatterns.SequenceStartValueFromInsert.Match(insertStatement);
        var sequenceStart =
            sequenceStartMatch.Success && long.TryParse(sequenceStartMatch.Groups[1].Value, out var parsed)
                ? parsed
                : 0;
        if (!string.IsNullOrEmpty(sequence.OwnerColumn))
        {
            var property = entityType.FindProperty(sequence.OwnerColumn);
            if (property != null)
            {
                property.Builder.HasSequence(sequence.Identifier.Name, sequence.Identifier.Schema)!
                    .StartsAt(sequenceStart);
                return;
            }
        }

        modelBuilder.HasSequence(sequence.Identifier.Name, sequence.Identifier.Schema, fromDataAnnotation: true)
            .StartsAt(sequenceStart);
    }

    private sealed class NullMigrator : Migrator
    {
        private const string ErrorDetails =
            "Entity creation uses a null instance of the Marten migrator in order to inspect Marten generated SQL. " +
            "This null migrator instance no longer works with the Marten API and needs to be updated";

        public NullMigrator() : base(string.Empty)
        {
        }

        public override void WriteScript(TextWriter writer, Action<Migrator, TextWriter> writeStep)
            => throw new MartenIntegrationException(MartenIntegrationStrings.MartenApiChange(ErrorDetails));

        public override void WriteSchemaCreationSql(IEnumerable<string> schemaNames, TextWriter writer)
            => throw new MartenIntegrationException(MartenIntegrationStrings.MartenApiChange(ErrorDetails));

        protected override Task executeDelta(SchemaMigration migration, DbConnection conn, AutoCreate autoCreate,
            IMigrationLogger logger,
            CancellationToken ct = new())
            => throw new MartenIntegrationException(MartenIntegrationStrings.MartenApiChange(ErrorDetails));

        public override string ToExecuteScriptLine(string scriptName)
            => throw new MartenIntegrationException(MartenIntegrationStrings.MartenApiChange(ErrorDetails));

        public override void AssertValidIdentifier(string name)
            => throw new MartenIntegrationException(MartenIntegrationStrings.MartenApiChange(ErrorDetails));
    }
}
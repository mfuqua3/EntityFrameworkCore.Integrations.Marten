using System.Text.Json;
using JasperFx.Core.Reflection;
using Marten.Schema;
using Marten.Storage.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Weasel.Postgresql;
using Weasel.Postgresql.Tables;
using SortOrder = Weasel.Postgresql.Tables.SortOrder;

namespace EntityFrameworkCore.Integrations.Marten.Metadata.Infrastructure;

public class MartenDocumentEntityTypeBuilder : IMartenDocumentEntityTypeBuilder
{
    private const string MartenPropertyPrefix = "mt__property__";

    public void ProcessMartenDocument(IConventionEntityTypeBuilder entityTypeBuilder, DocumentMapping documentMapping)
    {
        entityTypeBuilder.ToTable(documentMapping.TableName.Name, documentMapping.TableName.Schema);

        //Ignore all properties on the entity type
        foreach (var entityProperty in entityTypeBuilder.Metadata.ClrType.GetProperties())
        {
            entityTypeBuilder.Ignore(entityProperty.Name);
        }

        var propertyBuilderLookup =
            new Dictionary<string, IConventionPropertyBuilder>();
        var idProperty = entityTypeBuilder
            .Property(documentMapping.IdType, MartenPropertyPrefix + documentMapping.IdMember.Name)!
            .HasColumnName("id")!
            .HasColumnType(PostgresqlProvider.Instance.GetDatabaseType(documentMapping.IdMember.GetMemberType()!,
                documentMapping.EnumStorage))!;
        propertyBuilderLookup.Add("id", idProperty);
        entityTypeBuilder.PrimaryKey(new[] { idProperty.Metadata });
        foreach (var (name, metadataColumn) in GetDocumentMetadata(documentMapping.Metadata))
        {
            if (!metadataColumn.Enabled)
            {
                continue;
            }

            var propertyBuilder = entityTypeBuilder.Property(metadataColumn.DotNetType,
                    MartenPropertyPrefix + name)!
                .HasColumnType(metadataColumn.Type)!
                .HasColumnName(metadataColumn.Name)!;
            propertyBuilderLookup.Add(metadataColumn.Name, propertyBuilder);
            if (metadataColumn.Name == nameof(DocumentMetadataCollection.DocumentType) && documentMapping.IsHierarchy())
            {
                var index = new DocumentIndex(documentMapping, metadataColumn.Name);
                BuildIndex(entityTypeBuilder, new []{propertyBuilder}, index);
            }

            if (metadataColumn.Name == nameof(DocumentMetadataCollection.IsSoftDeleted) &&
                documentMapping.DeleteStyle == DeleteStyle.SoftDelete)
            {
                var index = new DocumentIndex(documentMapping, metadataColumn.Name);
                BuildIndex(entityTypeBuilder, new []{propertyBuilder}, index);
            }
        }

        var dataBuilder = entityTypeBuilder.Property(typeof(JsonDocument), MartenPropertyPrefix + "Data")!
            .HasColumnName("data")!
            .HasColumnType("jsonb")!;
        propertyBuilderLookup.Add("data", dataBuilder);

        foreach (var indexDefinition in documentMapping.Indexes)
        {
            var propertyBuilders = indexDefinition.Columns!.Select(c => propertyBuilderLookup[c]).ToArray();
            BuildIndex(entityTypeBuilder, propertyBuilders, indexDefinition);
        }

        foreach (var foreignKey in documentMapping.ForeignKeys)
        {
            var propertyBuilders = foreignKey.ColumnNames.Select(c => propertyBuilderLookup[c]).ToArray();
        }
    }

    private void BuildForeignKey(IConventionEntityTypeBuilder entityTypeBuilder, IConventionPropertyBuilder[] propertyBuilders,
        ForeignKey foreignKey)
    {
        //TODO
    }
    private void BuildIndex(IConventionEntityTypeBuilder entityTypeBuilder, IConventionPropertyBuilder[] propertyBuilders,
        IndexDefinition index)
    {
        var indexBuilder = entityTypeBuilder.HasIndex(propertyBuilders.Select(x=>x.Metadata.Name)!.ToArray(), index.Name)!
            .IsUnique(index.IsUnique)!
            .IsCreatedConcurrently(index.IsConcurrent)!
            .IsDescending(new[] { index.SortOrder == SortOrder.Desc })!
            .HasNullSortOrder(new[]
            {
                index.NullsSortOrder == NullsSortOrder.First
                    ? NullSortOrder.NullsFirst
                    : index.NullsSortOrder == NullsSortOrder.Last
                        ? NullSortOrder.NullsLast
                        : NullSortOrder.Unspecified
            }, false)!
            .HasMethod(index.Method.ToString())!;
        if (!string.IsNullOrEmpty(index.Collation))
        {
            indexBuilder.UseCollation(new[] { index.Collation }, false);
        }

        if (!string.IsNullOrEmpty(index.Predicate))
        {
            indexBuilder.HasFilter(index.Predicate);
        }
    }

    private IEnumerable<Tuple<string, MetadataColumn>> GetDocumentMetadata(
        DocumentMetadataCollection metadataCollection)
        => metadataCollection.GetType()
            .GetProperties()
            .Where(x => x.PropertyType == typeof(MetadataColumn))
            .Select(pi => Tuple.Create(pi.Name, (MetadataColumn)pi.GetValue(metadataCollection)!));
}
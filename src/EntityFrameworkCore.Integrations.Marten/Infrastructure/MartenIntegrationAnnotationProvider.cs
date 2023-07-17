using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using EntityFrameworkCore.Integrations.Marten.Utilities;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;

namespace EntityFrameworkCore.Integrations.Marten.Infrastructure;

[SuppressMessage("Usage", "EF1001:Internal EF Core API usage.")]
public class MartenIntegrationAnnotationProvider : NpgsqlAnnotationProvider
{
    public MartenIntegrationAnnotationProvider(RelationalAnnotationProviderDependencies dependencies) :
        base(dependencies)
    {
    }

    public override IEnumerable<IAnnotation> For(IRelationalModel model, bool designTime)
    {
        
        foreach (var annotation in base.For(model, designTime))
            yield return annotation;
    }

    public override IEnumerable<IAnnotation> For(ITable table, bool designTime)
    {
        var modelTable = table.EntityTypeMappings.First();
        if (modelTable.EntityType.IsMartenGenerated())
        {
            yield return new MartenManagedAnnotation();
        }

        foreach (var annotation in base.For(table, designTime))
            yield return annotation;
    }

    public override IEnumerable<IAnnotation> For(ITableIndex index, bool designTime)
    {
        var modelIndex = index.MappedIndexes.First();
        if (modelIndex.IsMartenComputedIndex())
        {
            foreach (var annotation in GetComputedIndexAnnotations(modelIndex))
                yield return annotation;
        }

        foreach (var annotation in base.For(index, designTime))
            yield return annotation;
    }

    private IEnumerable<IAnnotation> GetComputedIndexAnnotations(IReadOnlyIndex index)
    {
        yield return new Annotation(MartenIntegrationAnnotationNames.ComputedIndex.Type, "ComputedIndex");
        if (index.GetCasing() is { } casing)
        {
            yield return new Annotation(MartenIntegrationAnnotationNames.ComputedIndex.Casing, casing);
        }

        if (index.GetMartenCollation() is { } collation)
        {
            yield return new Annotation(MartenIntegrationAnnotationNames.ComputedIndex.Collation, collation);
        }

        if (index.GetColumns() is { } columns)
        {
            yield return new Annotation(MartenIntegrationAnnotationNames.ComputedIndex.Columns, columns);
        }

        if (index.GetCustomMethod() is { } customMethod)
        {
            yield return new Annotation(MartenIntegrationAnnotationNames.ComputedIndex.CustomMethod, customMethod);
        }

        if (index.GetIncludeColumns() is { } includeColumns)
        {
            yield return new Annotation(MartenIntegrationAnnotationNames.ComputedIndex.IncludeColumns, includeColumns);
        }

        if (index.GetMartenSortOrder() is { } sortOrder)
        {
            yield return new Annotation(MartenIntegrationAnnotationNames.ComputedIndex.SortOrder, sortOrder);
        }

        if (index.GetNullsSortOrder() is { } nullsSortOrder)
        {
            yield return new Annotation(MartenIntegrationAnnotationNames.ComputedIndex.NullsSortOrder, nullsSortOrder);
        }

        if (index.GetIsUnique() is { } isUnique)
        {
            yield return new Annotation(MartenIntegrationAnnotationNames.ComputedIndex.IsUnique, isUnique);
        }

        if (index.GetIsConcurrent() is { } isConcurrent)
        {
            yield return new Annotation(MartenIntegrationAnnotationNames.ComputedIndex.IsConcurrent, isConcurrent);
        }

        if (index.GetMask() is { } mask)
        {
            yield return new Annotation(MartenIntegrationAnnotationNames.ComputedIndex.Mask, mask);
        }

        if (index.GetTableSpace() is { } tableSpace)
        {
            yield return new Annotation(MartenIntegrationAnnotationNames.ComputedIndex.TableSpace, tableSpace);
        }

        if (index.GetPredicate() is { } predicate)
        {
            yield return new Annotation(MartenIntegrationAnnotationNames.ComputedIndex.Predicate, predicate);
        }

        if (index.GetFillFactor() is { } fillFactor)
        {
            yield return new Annotation(MartenIntegrationAnnotationNames.ComputedIndex.FillFactor, fillFactor);
        }

        if (index.GetMartenMethod() is { } method)
        {
            yield return new Annotation(MartenIntegrationAnnotationNames.ComputedIndex.Method, method);
        }
    }
}
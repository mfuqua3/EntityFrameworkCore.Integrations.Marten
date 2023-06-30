using Marten.Schema;
using Microsoft.EntityFrameworkCore.Metadata;
using Weasel.Postgresql.Tables;
using AnnotationNames = EntityFrameworkCore.Integrations.Marten.Utilities.MartenIntegrationAnnotationNames.ComputedIndex;
namespace EntityFrameworkCore.Integrations.Marten.Utilities;

public static class MartenIntegrationIndexExtensions
{
    private static IndexDefinition _default = new IndexDefinition("default");
    public static bool IsMartenComputedIndex(this IReadOnlyIndex index) =>
        index.FindAnnotation(AnnotationNames.Type) != null &&
        index.FindAnnotation(AnnotationNames.Type)!.Value!.ToString() == "ComputedIndex";

    public static int GetCasing(this IReadOnlyIndex index)
        => (int)(index.FindAnnotation(AnnotationNames.Casing)?.Value ?? 0);
    public static int GetMartenMethod(this IReadOnlyIndex index)
        => (int)(index.FindAnnotation(AnnotationNames.Method)?.Value ?? _default.Method);
    public static string? GetCustomMethod(this IReadOnlyIndex index)
        => (string?)index.FindAnnotation(AnnotationNames.CustomMethod)?.Value;
    public static int GetMartenSortOrder(this IReadOnlyIndex index)
        => (int)(index.FindAnnotation(AnnotationNames.SortOrder)?.Value ?? _default.SortOrder);
    public static int GetNullsSortOrder(this IReadOnlyIndex index)
        => (int)(index.FindAnnotation(AnnotationNames.NullsSortOrder)?.Value ?? _default.NullsSortOrder);
    public static bool GetIsUnique(this IReadOnlyIndex index)
        => (bool)(index.FindAnnotation(AnnotationNames.IsUnique)?.Value ?? _default.IsUnique);
    public static bool GetIsConcurrent(this IReadOnlyIndex index)
        => (bool)(index.FindAnnotation(AnnotationNames.IsConcurrent)?.Value ?? _default.IsConcurrent);
    public static string[]? GetColumns(this IReadOnlyIndex index)
        => (string[]?)(index.FindAnnotation(AnnotationNames.Columns)?.Value ?? _default.Columns);
    public static string[]? GetIncludeColumns(this IReadOnlyIndex index)
        => (string[]?)(index.FindAnnotation(AnnotationNames.IncludeColumns)?.Value ?? _default.IncludeColumns);
    public static string? GetMask(this IReadOnlyIndex index)
        => (string?)(index.FindAnnotation(AnnotationNames.Mask)?.Value ?? _default.Mask);
    public static string? GetTableSpace(this IReadOnlyIndex index)
        => (string?)(index.FindAnnotation(AnnotationNames.TableSpace)?.Value ?? _default.TableSpace);
    public static string? GetPredicate(this IReadOnlyIndex index)
        => (string?)(index.FindAnnotation(AnnotationNames.Predicate)?.Value ?? _default.Predicate);
    public static string? GetMartenCollation(this IReadOnlyIndex index)
        => (string?)(index.FindAnnotation(AnnotationNames.Collation)?.Value ?? _default.Collation);
    public static int? GetFillFactor(this IReadOnlyIndex index)
        => (int?)(index.FindAnnotation(AnnotationNames.FillFactor)?.Value ?? _default.FillFactor);
}
using EntityFrameworkCore.Integrations.Marten.Design;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Migrations;

public static class MigrationBuilderExtensions
{
    public static OperationBuilder<CreateComputedIndexOperation> CreateComputedIndex(this MigrationBuilder builder,
        string name,
        string table,
        string column,
        bool isUnique = false,
        string[]? includeColumns = null,
        int casing = 0,
        int method = 0,
        string? customMethod = null,
        int sortOrder = 0,
        int nullsSortOrder = 0,
        bool isConcurrent = false,
        string? mask = null,
        string? tableSpace = null,
        string? predicate = null,
        int? fillFactor = null,
        string? collation = null)
        => builder.CreateComputedIndex(name, table, new[] { column }, isUnique, includeColumns, casing, method,
            customMethod, sortOrder, nullsSortOrder, isConcurrent, mask, tableSpace, predicate, fillFactor, collation);

    public static OperationBuilder<CreateComputedIndexOperation> CreateComputedIndex(this MigrationBuilder builder,
        string name,
        string table,
        string[] columns,
        bool isUnique = false,
        string[]? includeColumns = null,
        int casing = 0,
        int method = 0,
        string? customMethod = null,
        int sortOrder = 0,
        int nullsSortOrder = 0,
        bool isConcurrent = false,
        string? mask = null,
        string? tableSpace = null,
        string? predicate = null,
        int? fillFactor = null,
        string? collation = null)
    {
        var operation = new CreateComputedIndexOperation
        {
            IsDestructiveChange = false,
            Name = name,
            Table = table,
            Columns = columns,
            IsUnique = isUnique,
            IncludeColumns = includeColumns,
            Casing = casing,
            Method = method,
            CustomMethod = customMethod,
            SortOrder = sortOrder,
            NullsSortOrder = nullsSortOrder,
            IsConcurrent = isConcurrent,
            Mask = mask,
            TableSpace = tableSpace,
            Predicate = predicate,
            FillFactor = fillFactor,
            Collation = collation
        };
        builder.Operations.Add(operation);
        return new OperationBuilder<CreateComputedIndexOperation>(operation);
    }
}
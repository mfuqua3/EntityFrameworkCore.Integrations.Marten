using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace EntityFrameworkCore.Integrations.Marten.Design;

public class CreateComputedIndexOperation : MigrationOperation
{
    public string Name { get; set; } = null!;
    public string[] Columns { get; set; } = null!;
    public string[]? IncludeColumns { get; set; }
    public int Casing { get; set; }
    public int Method { get; set; }
    public string? CustomMethod { get; set; }
    public int SortOrder { get; set; }
    public int NullsSortOrder { get; set; }
    public bool IsUnique { get; set; }
    public bool IsConcurrent { get; set; }
    public string? Mask { get; set; }
    public string? TableSpace { get; set; }
    public string? Predicate { get; set; }
    public int? FillFactor { get; set; }
    public string? Collation { get; set; }
    public string Table { get; set; } = null!;

    public static CreateComputedIndexOperation CreateFrom(ITableIndex index)
    {
        var operation = new CreateComputedIndexOperation
        {
            Name = index.Name,
            Table = index.Table.SchemaQualifiedName
        };
        operation.TrySetFromAnnotation(index, x => x.Columns);
        operation.TrySetFromAnnotation(index, x => x.IncludeColumns);
        operation.TrySetFromAnnotation(index, x => x.Casing);
        operation.TrySetFromAnnotation(index, x => x.Method);
        operation.TrySetFromAnnotation(index, x => x.CustomMethod);
        operation.TrySetFromAnnotation(index, x => x.SortOrder);
        operation.TrySetFromAnnotation(index, x => x.NullsSortOrder);
        operation.TrySetFromAnnotation(index, x => x.IsUnique);
        operation.TrySetFromAnnotation(index, x => x.IsConcurrent);
        operation.TrySetFromAnnotation(index, x => x.Mask);
        operation.TrySetFromAnnotation(index, x => x.TableSpace);
        operation.TrySetFromAnnotation(index, x => x.Predicate);
        operation.TrySetFromAnnotation(index, x => x.FillFactor);
        operation.TrySetFromAnnotation(index, x => x.Collation);
        return operation;
    }

    private void TrySetFromAnnotation<T>(ITableIndex index,
        Expression<Func<CreateComputedIndexOperation, T?>> propertyAccessExpression)
    {
        var expression = (MemberExpression)propertyAccessExpression.Body;
        var propertyInfo = (PropertyInfo)expression.Member;
        var name = propertyInfo.Name;
        var annotations = index.GetAnnotations().Where(annotation => annotation.Name.StartsWith("MartenComputedIndex"));
        var match = annotations.SingleOrDefault(annotation =>
            annotation.Name.Substring("MartenComputedIndex:".Length).Equals(name, StringComparison.OrdinalIgnoreCase));
        if (match == null)
        {
            return;
        }
        propertyInfo.SetValue(this, match.Value);
    }
}
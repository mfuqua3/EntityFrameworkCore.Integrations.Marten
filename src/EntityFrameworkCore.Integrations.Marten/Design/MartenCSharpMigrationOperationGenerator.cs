using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace EntityFrameworkCore.Integrations.Marten.Design;

public class MartenCSharpMigrationOperationGenerator : CSharpMigrationOperationGenerator
{
    public MartenCSharpMigrationOperationGenerator(CSharpMigrationOperationGeneratorDependencies dependencies) :
        base(dependencies)
    {
    }
    public override void Generate(string builderName, IReadOnlyList<MigrationOperation> operations, IndentedStringBuilder builder)
    {
        var first = true;
        foreach (var operation in operations)
        {
            if (first)
            {
                first = false;
            }
            else
            {
                builder
                    .AppendLine()
                    .AppendLine();
            }

            builder.Append(builderName);
            Generate((dynamic)operation, builder);
            builder.Append(";");
        }
        //base.Generate(builderName, operations, builder);
    }

    protected void Generate(CreateMartenTableFunctionsOperation operation, IndentedStringBuilder builder)
    {
        var code = Dependencies.CSharpHelper;
        builder.AppendLine(".CreateMartenTableFunctions(");
        using (builder.Indent())
        {
            builder
                .Append("schemaQualifiedTableName: ")
                .Append(code.Literal(operation.SchemaQualifiedTableName));
        }
        builder.Append(")");
    }

    protected void Generate(CreateMartenSystemFunctionsOperation operation, IndentedStringBuilder builder)
    {
        builder.Append(".CreateMartenSystemFunctions()");
    }
    
    protected void Generate(DropMartenTableFunctionsOperation operation, IndentedStringBuilder builder)
    {
        var code = Dependencies.CSharpHelper;
        builder.AppendLine(".DropMartenTableFunctions(");
        using (builder.Indent())
        {
            builder
                .Append("schemaQualifiedTableName: ")
                .Append(code.Literal(operation.SchemaQualifiedTableName));
        }
        builder.Append(")");
    }

    protected void Generate(CreateComputedIndexOperation operation, IndentedStringBuilder builder)
    {
        var code = Dependencies.CSharpHelper;
        builder.AppendLine(".CreateComputedIndex(");
        using (builder.Indent())
        {
            builder
                .Append("name: ")
                .Append(code.Literal(operation.Name));
            builder
                .AppendLine(",")
                .Append("table: ")
                .Append(code.Literal(operation.Table));
            
            builder.AppendLine(",");
            if (operation.Columns.Length == 1)
            {
                builder
                    .Append("column: ")
                    .Append(code.Literal(operation.Columns[0]));
            }
            else
            {
                builder
                    .Append("columns: ")
                    .Append("new [] { ")
                    .Append(string.Join(", ", operation.Columns.Select(code.Literal)))
                    .Append(" }");
            }

            if (operation.IsUnique)
            {
                builder
                    .AppendLine(",")
                    .Append("isUnique: true");
            }

            if (operation.IncludeColumns != null && operation.IncludeColumns.Length != 0)
            {
                builder
                    .AppendLine(",")
                    .Append("includeColumns: ")
                    .Append("new [] { ")
                    .Append(string.Join(", ", operation.IncludeColumns.Select(code.Literal)))
                    .Append(" }");
            }

            if (operation.Casing > 0)
            {
                builder
                    .AppendLine(",")
                    .Append("casing: ")
                    .Append(code.Literal(operation.Casing));
            }

            if (operation.Method > 0)
            {
                builder
                    .AppendLine(",")
                    .Append("method: ")
                    .Append(code.Literal(operation.Method));
            }

            if (operation.CustomMethod != null)
            {
                builder
                    .AppendLine(",")
                    .Append("customMethod: ")
                    .Append(code.Literal(operation.CustomMethod));
            }

            if (operation.SortOrder > 0)
            {
                builder
                    .AppendLine(",")
                    .Append("sortOrder: ")
                    .Append(code.Literal(operation.SortOrder));
            }

            if (operation.NullsSortOrder > 0)
            {
                builder
                    .AppendLine(",")
                    .Append("nullsSortOrder: ")
                    .Append(code.Literal(operation.NullsSortOrder));
            }

            if (operation.IsConcurrent)
            {
                builder
                    .AppendLine(",")
                    .Append("isConcurrent: true");
            }

            if (operation.Mask != null)
            {
                builder
                    .AppendLine(",")
                    .Append("mask: ")
                    .Append(code.Literal(operation.Mask));
            }

            if (operation.TableSpace != null)
            {
                builder
                    .AppendLine(",")
                    .Append("tableSpace: ")
                    .Append(code.Literal(operation.TableSpace));
            }

            if (operation.Predicate != null)
            {
                builder
                    .AppendLine(",")
                    .Append("predicate: ")
                    .Append(code.Literal(operation.Predicate));
            }

            if (operation.FillFactor != null)
            {
                builder
                    .AppendLine(",")
                    .Append("fillFactor: ")
                    .Append(code.Literal(operation.FillFactor));
            }

            if (operation.Collation != null)
            {
                builder
                    .AppendLine(",")
                    .Append("collation: ")
                    .Append(code.Literal(operation.Collation));
            }
            
            builder.Append(")");
        }
    }
}
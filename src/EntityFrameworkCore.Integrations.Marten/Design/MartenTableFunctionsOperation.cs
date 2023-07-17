using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace EntityFrameworkCore.Integrations.Marten.Design;

public abstract class MartenTableFunctionsOperation : MigrationOperation
{
    public string SchemaQualifiedTableName { get; set; }
}
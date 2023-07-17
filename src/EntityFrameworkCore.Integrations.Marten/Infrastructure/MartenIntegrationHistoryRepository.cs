using EntityFrameworkCore.Integrations.Marten.Metadata;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations.Internal;

namespace EntityFrameworkCore.Integrations.Marten.Infrastructure;

public class MartenIntegrationHistoryRepository : NpgsqlHistoryRepository
{
    private IModel? _model;
    public MartenIntegrationHistoryRepository(HistoryRepositoryDependencies dependencies) : base(dependencies)
    {
    }

    public override string GetCreateScript()
    {
        var model = EnsureModel();

        var operations = Dependencies.ModelDiffer.GetDifferences(null, model.GetRelationalModel());
        var commandList = Dependencies.MigrationsSqlGenerator.Generate(operations, model);

        return string.Concat(commandList.Select(c => c.CommandText));
    }
    
    private IModel EnsureModel()
    {
        if (_model == null)
        {
            var conventionSet = Dependencies.ConventionSetBuilder.CreateConventionSet();

            conventionSet.Remove(typeof(DbDocumentFindingConvention));
            conventionSet.Remove(typeof(DbSetFindingConvention));
            conventionSet.Remove(typeof(RelationalDbFunctionAttributeConvention));

            var modelBuilder = new ModelBuilder(conventionSet);
            modelBuilder.Entity<HistoryRow>(
                x =>
                {
                    ConfigureTable(x);
                    x.ToTable(TableName, TableSchema);
                });

            _model = Dependencies.ModelRuntimeInitializer.Initialize(
                (IModel)modelBuilder.Model, designTime: true, validationLogger: null);
        }

        return _model;
    }
}
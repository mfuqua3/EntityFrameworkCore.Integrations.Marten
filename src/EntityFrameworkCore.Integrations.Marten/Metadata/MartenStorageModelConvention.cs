using JasperFx.Core.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Weasel.Core.Migrations;
using Weasel.Postgresql;
using Weasel.Postgresql.Functions;
using Weasel.Postgresql.Tables;
using Weasel.Postgresql.Views;

namespace EntityFrameworkCore.Integrations.Marten.Metadata;

public class MartenStorageModelConvention : IModelInitializedConvention
{
    private const string MartenPropertyPrefix = "mt__property__";
    public MartenStorageModelConvention(MartenIntegrationConventionSetBuilderDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    protected virtual MartenIntegrationConventionSetBuilderDependencies Dependencies { get; }

    public void ProcessModelInitialized(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var documentInfo in Dependencies.DocumentFinder.FindDocuments(Dependencies.ContextType))
        {
            var documentMapping = Dependencies.DocumentMappingFactory.GetMapping(
                documentInfo.Type,
                Dependencies.StoreOptions);
            var feature = Dependencies.StoreOptions.Storage.FindFeature(documentInfo.Type);
            var entityTypeBuilder = modelBuilder.Entity(documentInfo.Type, fromDataAnnotation: true)!;
            // entityTypeBuilder.ToTable(documentMapping.TableName.Name, documentMapping.TableName.Schema);
            
            // foreach (var entityProperty in entityTypeBuilder.Metadata.ClrType.GetProperties())
            // {
            //     entityTypeBuilder.Ignore(entityProperty.Name);
            // }
            //
            // var idProperty = entityTypeBuilder
            //     .Property(documentMapping.IdType, "mt__property__" + documentMapping.IdMember.Name)!
            //     .HasColumnName("id")!
            //     .HasColumnType(PostgresqlProvider.Instance.GetDatabaseType(documentMapping.IdType,
            //         documentMapping.EnumStorage));
            // entityTypeBuilder.PrimaryKey(new[] { idProperty.Metadata });
            // entityTypeBuilder.ExcludeTableFromMigrations(true);
            entityTypeBuilder.HasAnnotation("EntityGenerationStrategy", "Marten");
            //Dependencies.EntityTypeBuilder.ProcessMartenDocument(entityTypeBuilder, documentMapping);
        }
    }

    private void ProcessFeature(IConventionModelBuilder modelBuilder, IFeatureSchema featureSchema, Type documentType)
    {
        var schemaObjects = featureSchema.Objects;
        foreach (var schemaObject in schemaObjects)
        {
            switch (schemaObject)
            {
                case Table table:
                    ProcessTable(modelBuilder, table, documentType);
                    break;
                case Function function:
                    ProcessFunction(modelBuilder, function, documentType);
                    break;
                case Sequence sequence:
                    ProcessSequence(modelBuilder, sequence, documentType);
                    break;
                case View view:
                    ProcessView(modelBuilder, view, documentType);
                    break;
            }
        }
    }

    private void ProcessTable(IConventionModelBuilder modelBuilder, Table table, Type documentType)
    {
        var entityTypeBuilder = modelBuilder.Entity(documentType, fromDataAnnotation: false);
        entityTypeBuilder.ToTable(table.Identifier.Name, table.Identifier.Schema);
        foreach (var entityProperty in entityTypeBuilder.Metadata.ClrType.GetProperties())
        {
            entityTypeBuilder.Ignore(entityProperty.Name);
        }
        
    }

    private void ProcessFunction(IConventionModelBuilder modelBuilder, Function function, Type documentType)
    {
        
    }
    private void ProcessSequence(IConventionModelBuilder modelBuilder, Sequence function, Type documentType)
    {
        
    }

    private void ProcessView(IConventionModelBuilder modelBuilder, View view, Type documentType)
    {
        
    }
}
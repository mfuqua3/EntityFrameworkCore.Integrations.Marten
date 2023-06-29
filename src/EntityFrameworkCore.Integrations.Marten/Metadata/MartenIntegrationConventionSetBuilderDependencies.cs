using EntityFrameworkCore.Integrations.Marten.Infrastructure;
using EntityFrameworkCore.Integrations.Marten.Metadata.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EntityFrameworkCore.Integrations.Marten.Metadata;

public sealed record MartenIntegrationConventionSetBuilderDependencies
{
    private readonly ICurrentDbContext _currentContext;

    public MartenIntegrationConventionSetBuilderDependencies(StoreOptions StoreOptions,
        IDocumentMappingFactory DocumentMappingFactory,
        IDbDocumentFinder DocumentFinder,
        IMartenDocumentEntityTypeBuilder EntityTypeBuilder,
        ICurrentDbContext currentContext)
    {
        this.StoreOptions = StoreOptions;
        this.DocumentMappingFactory = DocumentMappingFactory;
        this.DocumentFinder = DocumentFinder;
        this.EntityTypeBuilder = EntityTypeBuilder;
        _currentContext = currentContext;
    }

    public IMartenDocumentEntityTypeBuilder EntityTypeBuilder { get; init; }

    public StoreOptions StoreOptions { get; init; }
    public IDocumentMappingFactory DocumentMappingFactory { get; init; }
    public IDbDocumentFinder DocumentFinder { get; init; }
    public Type ContextType
        => _currentContext.Context.GetType();

    public MartenIntegrationConventionSetBuilderDependencies With(ICurrentDbContext currentContext)
        => new(
            StoreOptions,
            DocumentMappingFactory,
            DocumentFinder,
            EntityTypeBuilder,
            currentContext);
}
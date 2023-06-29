using EntityFrameworkCore.Integrations.Marten.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EntityFrameworkCore.Integrations.Marten.Infrastructure;

public sealed record DbDocumentMartenRegistryDependencies
{
    private readonly ICurrentDbContext _currentContext;
    public IDbDocumentFinder DocumentFinder { get; }

    public DbDocumentMartenRegistryDependencies(
        IDbDocumentFinder DocumentFinder,
        ICurrentDbContext currentContext)
    {
        _currentContext = currentContext;
        this.DocumentFinder = DocumentFinder;
    }
    
    public Type ContextType
        => _currentContext.Context.GetType();
}
public class DbDocumentMartenRegistry : MartenRegistry
{
    private readonly DbDocumentMartenRegistryDependencies _dependencies;

    public DbDocumentMartenRegistry(DbDocumentMartenRegistryDependencies dependencies)
    {
        _dependencies = dependencies;
        BuildRegistry();
    }

    private void BuildRegistry()
    {
        var documents = _dependencies.DocumentFinder.FindDocuments(_dependencies.ContextType);
        foreach (var document in documents)
        {
            GetType().GetMethod(nameof(For))!
                .MakeGenericMethod(document.Type)
                .Invoke(this, null);
        }
    }
}
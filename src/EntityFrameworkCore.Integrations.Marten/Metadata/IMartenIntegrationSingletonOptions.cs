namespace EntityFrameworkCore.Integrations.Marten.Metadata;

public interface IMartenIntegrationSingletonOptions
{
    public StoreOptions StoreOptions { get;  }
    
}
public sealed record MartenIntegrationSingletonDependencies(StoreOptions StoreOptions) : IMartenIntegrationSingletonOptions;

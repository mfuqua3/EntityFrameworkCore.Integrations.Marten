namespace EntityFrameworkCore.Integrations.Marten.Infrastructure;

public interface IDbDocumentInitializer
{
    void InitializeDocuments(MartenIntegratedDbContext context);
}
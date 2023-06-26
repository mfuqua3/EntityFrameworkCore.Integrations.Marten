using EntityFrameworkCore.Integrations.Marten.Internal;

namespace EntityFrameworkCore.Integrations.Marten.Infrastructure;

public class DbDocumentInitializer : IDbDocumentInitializer
{
    private readonly IDbDocumentFinder _documentFinder;
    private readonly IDbDocumentSource _documentSource;

    public DbDocumentInitializer(
        IDbDocumentFinder documentFinder,
        IDbDocumentSource documentSource)
    {
        _documentFinder = documentFinder;
        _documentSource = documentSource;
    }
    public virtual void InitializeDocuments(MartenIntegratedDbContext context)
    {
        foreach (var documentInfo in _documentFinder.FindDocuments(context.GetType()).Where(p=>p.Setter != null))
        {
            documentInfo.Setter!.SetClrValue(
                context, 
                ((IDbDocumentCache)context).GetOrAddDocument(_documentSource, documentInfo.Type));
        }
    }
}
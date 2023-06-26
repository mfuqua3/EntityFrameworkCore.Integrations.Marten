namespace EntityFrameworkCore.Integrations.Marten.Internal;

public interface IDbDocumentCache
{
    object GetOrAddDocument(IDbDocumentSource source, Type type);
    IEnumerable<object> GetDocuments();
}
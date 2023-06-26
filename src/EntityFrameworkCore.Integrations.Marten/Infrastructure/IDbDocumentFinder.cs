namespace EntityFrameworkCore.Integrations.Marten.Infrastructure;

public interface IDbDocumentFinder
{
    IReadOnlyList<DbDocumentProperty> FindDocuments(Type contextType);
}
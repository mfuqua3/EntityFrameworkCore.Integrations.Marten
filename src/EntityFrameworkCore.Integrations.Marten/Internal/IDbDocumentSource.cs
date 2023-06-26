namespace EntityFrameworkCore.Integrations.Marten.Internal;

public interface IDbDocumentSource
{
    object Create(DbContext context, Type type);
}
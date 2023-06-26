using System.Runtime.Serialization;

namespace EntityFrameworkCore.Integrations.Marten.Exceptions;
[Serializable]
public class DbDocumentInitializationException: MartenIntegrationException
{
    public DbDocumentInitializationException(string? message): base(message)
    {
    }

    protected internal DbDocumentInitializationException(SerializationInfo info, StreamingContext context) : base(info, context) {}
}
using System.Runtime.Serialization;

namespace EntityFrameworkCore.Integrations.Marten.Exceptions;

[Serializable]
public class MartenEntityBuilderException : MartenIntegrationException
{
    public MartenEntityBuilderException(string message): base(message)
    {
        
    }

    public MartenEntityBuilderException(string message, Exception inner) : base(message, inner)
    {
        
    }
    protected internal MartenEntityBuilderException(SerializationInfo info, StreamingContext context) : base(info,
        context)
    {
    }
}
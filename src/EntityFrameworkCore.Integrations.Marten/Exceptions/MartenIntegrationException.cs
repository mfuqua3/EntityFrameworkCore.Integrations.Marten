using System.Runtime.Serialization;

namespace EntityFrameworkCore.Integrations.Marten.Exceptions;

[Serializable]
public class MartenIntegrationException : Exception
{
    public MartenIntegrationException()
    {
        
    }
    public MartenIntegrationException(string? message): base(message)
    {
        
    }

    public MartenIntegrationException(string? message, Exception? inner):base(message, inner)
    {
        
    }
    #region Serialization

    /// <summary>
    /// Initializes a new instance of the <see cref="MartenIntegrationException"/> class with serialized data.
    /// </summary>
    /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
    /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
    protected internal MartenIntegrationException(SerializationInfo info, StreamingContext context) : base(info, context) {}

    #endregion
}
using System.Reflection;
using System.Text.Json;

namespace EntityFrameworkCore.Integrations.Marten.Metadata.Infrastructure;

public class MartenDocument
{
    public object? Id { get; set; }
    public DateTimeOffset? LastModified { get; set; }
    public string? DocumentType { get; set; }
    public Guid? Version { get; set; }
    public string? TenantId { get; set; }
    public bool? IsSoftDeleted { get; set; }
    public DateTimeOffset? SoftDeletedAt { get; set; }
    public string? DotNetType { get; set; }
    public string? CausationId { get; set; }
    public string? CorrelationId { get; set; }
    public string? LastModifiedBy { get; set; }
    public JsonDocument? Headers { get; set; }
    public JsonDocument? Data { get; set; }

    internal static MartenDocument<T> GenericFor<T>()
    {
        return new MartenDocument<T>();
    }

    internal static MartenDocument For(Type documentType)
        => (MartenDocument)(typeof(MartenDocument)
            .GetMethod(nameof(GenericFor), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(documentType).Invoke(null, null))!;
}
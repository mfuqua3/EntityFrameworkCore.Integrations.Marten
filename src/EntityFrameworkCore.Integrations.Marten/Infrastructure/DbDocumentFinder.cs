using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace EntityFrameworkCore.Integrations.Marten.Infrastructure;

public class DbDocumentFinder : IDbDocumentFinder
{
    private readonly ConcurrentDictionary<Type, IReadOnlyList<DbDocumentProperty>> _cache = new();
    public IReadOnlyList<DbDocumentProperty> FindDocuments(Type contextType)
        => _cache.GetOrAdd(contextType, FindDocumentsNonCached);
    
    [SuppressMessage("Usage", "EF1001:Internal EF Core API usage.", Justification = "Reviewed")]
    private static DbDocumentProperty[] FindDocumentsNonCached(Type contextType)
    {
        var factory = new ClrPropertySetterFactory();

        return contextType.GetRuntimeProperties()
            .Where(
                p => !p.IsStatic()
                     && !p.GetIndexParameters().Any()
                     && p.DeclaringType != typeof(DbContext)
                     && p.PropertyType.GetTypeInfo().IsGenericType
                     && p.PropertyType.GetGenericTypeDefinition() == typeof(DbDocument<>))
            .OrderBy(p => p.Name)
            .Select(
                p => new DbDocumentProperty(p.PropertyType.GenericTypeArguments.Single(),
                    p.SetMethod == null ? null : factory.Create(p)))
            .ToArray();
    }
}
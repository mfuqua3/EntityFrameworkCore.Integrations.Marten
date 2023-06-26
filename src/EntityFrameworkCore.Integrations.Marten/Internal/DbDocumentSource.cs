using System.Collections.Concurrent;
using System.Reflection;

namespace EntityFrameworkCore.Integrations.Marten.Internal;

public class DbDocumentSource : IDbDocumentSource
{
    private static readonly MethodInfo GenericCreateDocument
        = typeof(DbDocumentSource).GetTypeInfo().GetDeclaredMethod(nameof(CreateDocumentFactory))!;
    private readonly ConcurrentDictionary<Type, Func<DbContext, object>> _cache = new();

    public object Create(DbContext context, Type type)
        => CreateCore(context, type, GenericCreateDocument);
    private object CreateCore(DbContext context, Type type, MethodInfo createMethod)
        => _cache.GetOrAdd(
            (type),
            static (t, createMethod) => (Func<DbContext, object>)createMethod
                .MakeGenericMethod(t)
                .Invoke(null, null)!,
            createMethod)(context);
    
    private static Func<DbContext, object> CreateDocumentFactory<TEntity>()
        where TEntity : class
        => (c) => new InternalDbDocument<TEntity>(c);
}
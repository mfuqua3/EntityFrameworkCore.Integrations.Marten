using System.Collections.Concurrent;
using Marten.Schema;

namespace EntityFrameworkCore.Integrations.Marten.Metadata.Infrastructure;

public class DocumentMappingFactory : IDocumentMappingFactory
{
    private readonly ConcurrentDictionary<Type, DocumentMapping> _cache = new();

    public virtual DocumentMapping GetMapping(Type documentType, StoreOptions storeOptions)
        => _cache.GetOrAdd(documentType, type => new DocumentMapping(type, storeOptions));
}
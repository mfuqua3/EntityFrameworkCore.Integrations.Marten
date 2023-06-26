using Marten.Schema;

namespace EntityFrameworkCore.Integrations.Marten.Metadata.Infrastructure;

public interface IDocumentMappingFactory
{
    DocumentMapping GetMapping(Type documentType, StoreOptions storeOptions);
}
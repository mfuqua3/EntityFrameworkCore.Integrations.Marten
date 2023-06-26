using Marten.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EntityFrameworkCore.Integrations.Marten.Metadata.Infrastructure;

public interface IMartenDocumentEntityTypeBuilder
{
    void ProcessMartenDocument(IConventionEntityTypeBuilder entityTypeBuilder, DocumentMapping documentMapping);
}
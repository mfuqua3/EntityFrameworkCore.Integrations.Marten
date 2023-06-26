using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace EntityFrameworkCore.Integrations.Marten.Migrations;

[SuppressMessage("Usage", "EF1001:Internal EF Core API usage.")]
public class MartenRelationalModel : RelationalModel
{
    public MartenRelationalModel(IModel model) : base(model)
    {
    }
    
}
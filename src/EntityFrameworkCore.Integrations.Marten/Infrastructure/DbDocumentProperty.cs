using Microsoft.EntityFrameworkCore.Metadata;

namespace EntityFrameworkCore.Integrations.Marten.Infrastructure;

public readonly struct DbDocumentProperty
{
    public Type Type { get; }
    public IClrPropertySetter? Setter { get; }

    public DbDocumentProperty(Type type, IClrPropertySetter? setter)
    {
        Type = type;
        Setter = setter;
    }
}
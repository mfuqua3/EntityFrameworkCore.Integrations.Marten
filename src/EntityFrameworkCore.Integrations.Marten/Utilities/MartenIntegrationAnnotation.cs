using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EntityFrameworkCore.Integrations.Marten.Utilities;

public class MartenIntegrationAnnotation : Annotation
{
    public MartenIntegrationAnnotation(string version):base(MartenIntegrationAnnotationNames.MartenIntegration, version)
    {
        
    }
}
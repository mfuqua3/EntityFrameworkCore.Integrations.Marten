using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EntityFrameworkCore.Integrations.Marten.Utilities;

public class MartenManagedAnnotation : Annotation
{
    public MartenManagedAnnotation() : base(MartenIntegrationAnnotationNames.EntityManagement,
        MartenIntegrationAnnotationValues.MartenEntityManagement)
    {
    }
}
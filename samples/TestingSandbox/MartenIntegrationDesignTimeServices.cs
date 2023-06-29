using EntityFrameworkCore.Integrations.Marten.Infrastructure;
using EntityFrameworkCore.Integrations.Marten.Internal;
using EntityFrameworkCore.Integrations.Marten.Metadata;
using EntityFrameworkCore.Integrations.Marten.Metadata.Infrastructure;
using Marten;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace TestingSandbox;

internal class MartenIntegrationDesignTimeServices : IDesignTimeServices
{
    public void ConfigureDesignTimeServices(IServiceCollection serviceCollection)
    {
        //Console.WriteLine("WE HERE");
        // serviceCollection
        //     .AddSingleton<IDbDocumentFinder, DbDocumentFinder>()
        //     .AddSingleton<IDbDocumentInitializer, DbDocumentInitializer>()
        //     .AddSingleton<IDbDocumentSource, DbDocumentSource>()
        //     .AddSingleton<IDocumentMappingFactory, DocumentMappingFactory>()
        //     .AddSingleton<IMartenDocumentEntityTypeBuilder, MartenDocumentEntityTypeBuilder>()
        //     .AddScoped<MartenIntegrationConventionSetBuilderDependencies>()
        //     .AddScoped<IConventionSetPlugin, MartenIntegrationConventionSetPlugin>()
        //     .AddMarten(x =>
        //     {
        //         x.Connection("Server=localhost;Database=martenIntegrationDb;User Id=postgres;password=password;");
        //     });
    }
}
//
// internal class MartenIntegrationDesignTimeModel : IDesignTimeModel
// {
//     public IModel Model { get; }
// }
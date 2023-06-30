using EntityFrameworkCore.Integrations.Marten.Design;
using EntityFrameworkCore.Integrations.Marten.Infrastructure;
using EntityFrameworkCore.Integrations.Marten.Internal;
using EntityFrameworkCore.Integrations.Marten.Metadata;
using EntityFrameworkCore.Integrations.Marten.Metadata.Infrastructure;
using EntityFrameworkCore.Integrations.Marten.Utilities;
using Marten;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace TestingSandbox;

internal class MartenIntegrationDesignTimeServices : IDesignTimeServices
{
    public void ConfigureDesignTimeServices(IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddSingleton<ICSharpMigrationOperationGenerator, MartenCSharpMigrationOperationGenerator>();
    }
}
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EntityFrameworkCore.Integrations.Marten.Infrastructure;

public class MartenIntegratedDbContextOptionsBuilder
{
    private readonly DbContextOptionsBuilder _optionsBuilder;

    public MartenIntegratedDbContextOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
    {
        _optionsBuilder = optionsBuilder;
    }

    public MartenIntegratedDbContextOptionsBuilder WithPolicies(
        Action<StoreOptions.PoliciesExpression> configurePolicies) =>
        WithOption(ext => configurePolicies.Invoke(ext.Policies));
    public int NameDataLength
    {
        set
        {
            WithOption(ext => ext.NameDataLength = value);
        }
    }

    public string SchemaName
    {
        set
        {
            WithOption(ext => ext.SchemaName = value);
        }
    }

    private MartenIntegratedDbContextOptionsBuilder WithOption(
        Action<MartenIntegrationExtension> setAction)
        => WithOption((ext) =>
        {
            setAction(ext);
            return ext;
        });
    private MartenIntegratedDbContextOptionsBuilder WithOption(
        Func<MartenIntegrationExtension, MartenIntegrationExtension> setAction)
    {
        ((IDbContextOptionsBuilderInfrastructure)_optionsBuilder).AddOrUpdateExtension(
            setAction(_optionsBuilder.Options.FindExtension<MartenIntegrationExtension>() ??
                      new MartenIntegrationExtension()));
        return this;
    }
}
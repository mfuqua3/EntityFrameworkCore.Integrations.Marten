namespace EntityFrameworkCore.Integrations.Marten;

/// <summary>
/// The base DbContext for integrating Marten with Entity Framework Core. This class should be used as the base class
/// for your application-specific DbContext.
/// </summary>
public class MartenIntegratedDbContext : DbContext
{
    /// <summary>
    /// Default constructor that initializes a new instance of the MartenIntegratedDbContext class.
    /// </summary>
    public MartenIntegratedDbContext() : this(new DbContextOptions<MartenIntegratedDbContext>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the MartenIntegratedDbContext class using the specified options. This constructor
    /// is called by derived context classes.
    /// </summary>
    /// <param name="dbContextOptions">The options to be used by a DbContext. You normally override OnConfiguring or use
    /// a DbContextOptionsBuilder to replace or augment the created options.</param>
    public MartenIntegratedDbContext(DbContextOptions dbContextOptions) : base((dbContextOptions))
    {
    }
}

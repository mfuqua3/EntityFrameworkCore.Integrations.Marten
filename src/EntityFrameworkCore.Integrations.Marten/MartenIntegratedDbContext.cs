using System.Diagnostics.CodeAnalysis;
using EntityFrameworkCore.Integrations.Marten.Infrastructure;
using EntityFrameworkCore.Integrations.Marten.Internal;
using EntityFrameworkCore.Integrations.Marten.Metadata;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace EntityFrameworkCore.Integrations.Marten;

/// <summary>
/// The base DbContext for integrating Marten with Entity Framework Core. This class should be used as the base class
/// for your application-specific DbContext.
/// </summary>
public class MartenIntegratedDbContext : DbContext, IDbDocumentCache
{
    private bool _disposed;
    private IDictionary<Type, object>? _documents;

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
    /// <param name="options">The options to be used by a DbContext. You normally override OnConfiguring or use
    /// a DbContextOptionsBuilder to replace or augment the created options.</param>
    [SuppressMessage("Usage", "EF1001:Internal EF Core API usage.")]
    public MartenIntegratedDbContext(DbContextOptions options) : base(options)
    {
        ServiceProviderCache.Instance.GetOrAdd(options, providerRequired: false)
            .GetRequiredService<IDbDocumentInitializer>()
            .InitializeDocuments(this);
    }

    object IDbDocumentCache.GetOrAddDocument(IDbDocumentSource source, Type type)
    {
        CheckDisposed();
        _documents ??= new Dictionary<Type, object>();
        if (!_documents.TryGetValue(type, out var document))
        {
            document = source.Create(this, type);
            _documents[type] = document;
        }

        return document;
    }

    IEnumerable<object> IDbDocumentCache.GetDocuments()
        => _documents?.Values ?? Enumerable.Empty<object>();

    public override void Dispose()
    {
        base.Dispose();
        _disposed = true;
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync().ConfigureAwait(false);
        _disposed = true;
    }

    private void CheckDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(GetType().ShortDisplayName(), CoreStrings.ContextDisposed);
        }
    }
}
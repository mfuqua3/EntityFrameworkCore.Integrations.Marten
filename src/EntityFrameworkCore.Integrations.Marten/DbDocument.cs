using System.Collections;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EntityFrameworkCore.Integrations.Marten;

/// <summary>
///     A <see cref="DbDocument{TEntity}" /> can be used to query and save instances of <typeparamref name="TEntity" />.
///     LINQ queries against a <see cref="DbDocument{TEntity}" /> will be translated into queries against the document database using Marten.
/// </summary>
/// <remarks>
///     <para>
///         The results of a LINQ query against a <see cref="DbDocument{TEntity}" /> will contain the results
///         returned from the document table and may not reflect changes made in the context that have not
///         been persisted to the database. For example, the results will not contain newly added documents
///         and may still contain documents that are marked for deletion.
///     </para>
///     <para>
///         Depending on the Marten configuration being used, some parts of a LINQ query against a <see cref="DbDocument{TEntity}" />
///         may be evaluated in memory rather than being translated into a database query.
///     </para>
///     <para>
///         <see cref="DbDocument{TEntity}" /> objects are usually obtained from a <see cref="DbDocument{TEntity}" />
///         property on a derived <see cref="MartenIntegratedDbContext" />.
///     </para>
///     <para>
///         Marten does not support multiple parallel operations being run on the same <see cref="IDocumentSession" />
///         instance. Therefore, always await async calls immediately, or use separate DocumentSession instances for operations that execute
///         in parallel.
///     </para>
///     <para>
///         See Marten's documentation for more information and examples.
///     </para>
/// </remarks>
/// <typeparam name="TEntity">The type of entity being operated on by this set.</typeparam>
public abstract class DbDocument<TEntity> : IQueryable<TEntity>,
    IAsyncEnumerable<TEntity> where TEntity : class
{
    /// <summary>
    ///     Returns this object typed as <see cref="IAsyncEnumerable{T}" />.
    /// </summary>
    /// <returns>This object.</returns>
    public virtual IAsyncEnumerable<TEntity> AsAsyncEnumerable()
        => (IAsyncEnumerable<TEntity>)this;
    /// <summary>
    ///     Returns this object typed as <see cref="IQueryable{T}" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This is a convenience method to help with disambiguation of extension methods in the same
    ///         namespace that extend both interfaces.
    ///     </para>
    /// </remarks>
    /// <returns>This object.</returns>
    public virtual IQueryable<TEntity> AsQueryable()
        => this;
    /// <summary>
    ///     Returns an <see cref="IEnumerator{T}" /> which when enumerated will execute a query against the database
    ///     to load all entities from the database.
    /// </summary>
    /// <returns>The query results.</returns>
    IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator()
        => throw new NotSupportedException();
    /// <summary>
    ///     Returns an <see cref="IEnumerator" /> which when enumerated will execute a query against the database
    ///     to load all entities from the database.
    /// </summary>
    /// <returns>The query results.</returns>
    IEnumerator IEnumerable.GetEnumerator()
        => throw new NotSupportedException();
    /// <summary>
    ///     Gets the IQueryable element type.
    /// </summary>
    Type IQueryable.ElementType 
        => throw new NotSupportedException();
    /// <summary>
    ///     Gets the IQueryable LINQ Expression.
    /// </summary>
    Expression IQueryable.Expression 
        => throw new NotSupportedException();
    /// <summary>
    ///     Gets the IQueryable provider.
    /// </summary>
    IQueryProvider IQueryable.Provider 
        => throw new NotSupportedException();
    public virtual void Add(TEntity entity)
        => throw new NotSupportedException();
    public virtual void Remove(TEntity entity)
        => throw new NotSupportedException();
    public virtual void Update(TEntity entity)
        => throw new NotSupportedException();
    public virtual void Upsert(TEntity entity)
        => throw new NotSupportedException();
    public virtual void AddRange(params TEntity[] entities)
        => throw new NotSupportedException();
    public virtual void AddRange(IEnumerable<TEntity> entities)
        => throw new NotSupportedException();
    public virtual void RemoveRange(params TEntity[] entities)
        => throw new NotSupportedException();
    public virtual void RemoveRange(IEnumerable<TEntity> entities)
        => throw new NotSupportedException();
    public virtual void UpdateRange(params TEntity[] entities)
        => throw new NotSupportedException();
    public virtual void UpdateRange(IEnumerable<TEntity> entities)
        => throw new NotSupportedException();
    public virtual void UpsertRange(params TEntity[] entities)
        => throw new NotSupportedException();
    public virtual void UpsertRange(IEnumerable<TEntity> entities)
        => throw new NotSupportedException();

    public IAsyncEnumerator<TEntity> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
        => throw new NotSupportedException();
}
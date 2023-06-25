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
public abstract class DbDocument<TEntity> where TEntity : class
{
}
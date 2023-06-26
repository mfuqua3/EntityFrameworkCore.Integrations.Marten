using System.Collections;
using System.Linq.Expressions;
using Marten.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EntityFrameworkCore.Integrations.Marten.Internal;

public class InternalDbDocument<TEntity> : DbDocument<TEntity>, IQueryable<TEntity>,
    IAsyncEnumerable<TEntity> where TEntity : class
{
    private readonly IDocumentSession _documentSession;

    public InternalDbDocument(DbContext dbContext)
    {
        _documentSession = dbContext.GetService<IDocumentSession>();
    }

    private IMartenQueryable<TEntity> EntityQueryable
        => _documentSession.Query<TEntity>();

    IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator()
        => EntityQueryable.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => EntityQueryable.GetEnumerator();

    Type IQueryable.ElementType
        => EntityQueryable.ElementType;

    Expression IQueryable.Expression =>
        EntityQueryable.Expression;

    IQueryProvider IQueryable.Provider =>
        EntityQueryable.Provider;

    IAsyncEnumerator<TEntity> IAsyncEnumerable<TEntity>.GetAsyncEnumerator(CancellationToken cancellationToken)
        => EntityQueryable.AsAsyncEnumerable().GetAsyncEnumerator(cancellationToken);

    public override void Add(TEntity entity) => 
        _documentSession.Insert(entity);

    public override void Remove(TEntity entity)
        => _documentSession.Delete(entity);

    public override void Update(TEntity entity)
        => _documentSession.Update(entity);

    public override void Upsert(TEntity entity)
        => _documentSession.Store(entity);

    public override void AddRange(params TEntity[] entities)
        => _documentSession.InsertObjects(entities);

    public override void AddRange(IEnumerable<TEntity> entities)
        => _documentSession.InsertObjects(entities);

    public override void RemoveRange(params TEntity[] entities)
        => _documentSession.DeleteObjects(entities);

    public override void RemoveRange(IEnumerable<TEntity> entities)
        => _documentSession.DeleteObjects(entities);

    public override void UpdateRange(params TEntity[] entities)
        => _documentSession.Update(entities);

    public override void UpdateRange(IEnumerable<TEntity> entities)
        => _documentSession.Update(entities);

    public override void UpsertRange(params TEntity[] entities)
        => _documentSession.Store(entities);

    public override void UpsertRange(IEnumerable<TEntity> entities)
        => _documentSession.Store(entities);
}
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MyStoreManagement.Application.Interfaces.Repositories;
using MyStoreManagement.Application.Utils;
using MyStoreManagement.Application.Utils.Paganitions;
using MyStoreManagement.Infrastructure.Contexts;

namespace MyStoreManagement.Infrastructure.Repositories;

public class BaseRepository<TEntity>(MyStoreManagementContext context) : IRepository<TEntity> where TEntity : class
{
    private DbSet<TEntity> DbSet => context.Set<TEntity>();

    /// <summary>
    /// Find a record
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>>? predicate = null, bool isTracking = false, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes)
    {
        var query = DbSet.AsQueryable();
        if (predicate != null)
            query = query.Where(predicate);

        query = includes.Aggregate(query, (current, inc) => current.Include(inc));

        if (!isTracking) query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Find all records
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IEnumerable<TEntity>> ToListAsync(Expression<Func<TEntity, bool>>? predicate = null, bool isTracking = false, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes)
    {
        var query = DbSet.AsQueryable();
        if (predicate != null)
            query = query.Where(predicate);

        query = includes.Aggregate(query, (current, inc) => current.Include(inc));

        if (!isTracking) query = query.AsNoTracking();

        return await query.ToListAsync(cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Get paged records
    /// </summary>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <param name="predicate"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<PagedResult<TEntity>> PagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<TEntity, bool>>? predicate = null,
        bool isTracking = false,
        CancellationToken cancellationToken = default,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        params Expression<Func<TEntity, object>>[] includes
    )
    {
        var query = DbSet.AsQueryable();

        if (predicate != null)
            query = query.Where(predicate);

        query = includes.Aggregate(query, (current, inc) => current.Include(inc));

        if (!isTracking)
            query = query.AsNoTracking();

        if (orderBy != null)
            query = orderBy(query);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<TEntity>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
    
    /// <summary>
    /// Check if an entity exists based on the provided predicate.
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(predicate, cancellationToken);
    }
    
    /// <summary>
    /// Count entities based on the provided predicate.
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        if (predicate != null)
            return await DbSet.CountAsync(predicate, cancellationToken);
    
        return await DbSet.CountAsync(cancellationToken);
    }

    /// <summary>
    /// Add entity to the database
    /// </summary>
    /// <param name="entity"></param>
    public async Task AddAsync(TEntity entity)
    {
        await context.AddAsync(entity);
    }

    /// <summary>
    /// Add a range of entities to the database asynchronously
    /// </summary>
    /// <param name="entities"></param>
    public async Task AddRangeAsync(IEnumerable<TEntity> entities)
    {
        await context.AddRangeAsync(entities);
    }

    /// <summary>
    /// Update entity in the database
    /// </summary>
    /// <param name="entity"></param>
    public void Update(TEntity entity)
    {
        DbSet.Update(entity);
    }

    /// <summary>
    /// Update a range of entities in the database
    /// </summary>
    /// <param name="entities"></param>
    public void UpdateRange(IEnumerable<TEntity> entities)
    {
        DbSet.UpdateRange(entities);
    }

    /// <summary>
    /// Delete entity in the database
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public bool Delete(TEntity entity)
    {
        DbSet.Remove(entity);
        return true;
    }
}
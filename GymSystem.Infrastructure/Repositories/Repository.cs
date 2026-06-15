using GymSystem.Infrastructure.Data;
using GymSystem.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GymSystem.Infrastructure.Repositories;

public class Repository<TEntity>(GymAppDbContext dbContext) : IRepository<TEntity> where TEntity : BaseEntity
{
    private readonly GymAppDbContext _dbContext = dbContext;
    private readonly DbSet<TEntity> _dbSet = dbContext.Set<TEntity>();

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken ct = default)
        => await _dbSet.AsNoTracking().ToListAsync(ct);

    // instead of using multiple Include() statements in the service layer,
    // we can create a method in the repository that accepts an array of strings representing the navigation properties to include.
    // better method name than GetEntityIncludeEntityAsync, maybe GetAllWithIncludesAsync or GetAllIncludingAsync

    /// <summary>
    /// Retrieves all entities from the database with eager loading of related navigational properties.
    /// </summary>
    /// <param name="includes">An array of navigation property paths (as strings) to be included in the query results.</param>
    /// <param name="ct">A token to monitor for cancellation requests during the database operation.</param>
    /// <returns>A read-only list containing all retrieved entities, including their specified related data.</returns>
    public async Task<IReadOnlyList<TEntity>> GetAllIncludingAsync(string[] includes, CancellationToken ct = default)
    {
        var query = _dbSet.AsQueryable();

        foreach (var item in includes)
            query = query.Include(item);

        return await query.ToListAsync(ct);
    }

    public async Task<TEntity?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _dbSet.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<TEntity?> GetByIdTrackingIncludingAsync(
        int id,
        bool trackChanges = true,
        Expression<Func<TEntity, object?>>[]? includes = null,
        CancellationToken ct = default)
    {
        IQueryable<TEntity> query = _dbContext.Set<TEntity>();

        if (!trackChanges)
            query = query.AsNoTracking();

        if (includes is not null)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }

        return await query.FirstOrDefaultAsync(e => e.Id == id, ct);
    }


    // Why IgnoreQueryFilters()? 
    //      Cuz we want to include soft-deleted entities in the result, which are filtered out by global query filters.
    public async Task<TEntity?> GetByIdIncludingDeletedAsync(int id, CancellationToken ct = default)
        => await _dbSet.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
        => await _dbSet.AnyAsync(predicate, ct);

    // If your Controller/Service layer calls FindAsync to look up a record with the intention of modifying or deleting it right after,
    // .AsNoTracking() will cause _context.SaveChanges() to ignore those edits.
    public async Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
        => await _dbSet.Where(predicate).AsNoTracking().ToListAsync(ct);

    public async Task<IReadOnlyList<TEntity>> FindTrackedAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
        => await _dbSet.Where(predicate).ToListAsync(ct);



    public async Task AddAsync(TEntity entity, CancellationToken ct = default)
        => await _dbSet.AddAsync(entity, ct);



    // If your controller passes an untracked entity (for example, an entity rebuilt from an HTTP POST request form),
    // EF Core will throw an InvalidOperationException saying it cannot find the entity in the tracker store.How
    public Task SoftDeleteAsync(TEntity entity, CancellationToken ct = default)
    {
        entity.IsDeleted = true;

        // Check if EF Core is already tracking this object
        var entry = _dbContext.Entry(entity);

        // This brings it into the tracker without marking all columns as modified!
        if (entry.State == EntityState.Detached)
            _dbSet.Attach(entity);

        // is better than => _dbSet.Update(entity);
        _dbContext.Entry(entity).Property(x => x.IsDeleted).IsModified = true;
        //    this ensures that only the IsDeleted field is updated in the database,
        //      rather than all fields of the entity: Update(entity)
        //    This can be more efficient and reduces the risk of accidentally overwriting
        //      other fields if the entity has been modified elsewhere in the code.

        return Task.CompletedTask;
    }

    

    public void Update(TEntity entity, CancellationToken ct = default)
        => _dbSet.Update(entity);



    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _dbContext.SaveChangesAsync(ct);
}
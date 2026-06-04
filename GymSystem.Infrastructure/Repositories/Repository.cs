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
    public async Task<TEntity?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _dbSet.FirstOrDefaultAsync(x => x.Id == id, ct);

    // Why IgnoreQueryFilters()? 
    //      Cuz we want to include soft-deleted entities in the result, which are filtered out by global query filters.
    public async Task<TEntity?> GetByIdIncludingAsync(int id, CancellationToken ct = default)
        => await _dbSet.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<bool> ExistsAsync(int id, CancellationToken ct = default)
        => await _dbSet.AnyAsync(x => x.Id == id, ct);

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
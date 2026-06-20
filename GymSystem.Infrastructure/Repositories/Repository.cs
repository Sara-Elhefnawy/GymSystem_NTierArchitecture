using GymSystem.Domain.Abstractions.Repositories;
using GymSystem.Infrastructure.Data;
using GymSystem.Domain.Entities;
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

    public async Task AddAsync(TEntity entity, CancellationToken ct = default)
        => await _dbSet.AddAsync(entity, ct);

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

    public async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken ct = default)
    {
        var query = _dbSet.AsQueryable();

        if (predicate is not null)
            query = query.Where(predicate);

        return await query.CountAsync(ct);
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _dbContext.SaveChangesAsync(ct);
}
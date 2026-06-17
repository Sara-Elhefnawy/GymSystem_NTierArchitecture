using GymSystem.Infrastructure.Entities;
using System.Linq.Expressions;

namespace GymSystem.Infrastructure.Repositories;

public interface IRepository<TEntity> where TEntity : BaseEntity
{
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken ct = default );

    //Task<IReadOnlyList<TEntity>> GetAllIncludingAsync(string[] includes, CancellationToken ct = default);

    Task<TEntity?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<TEntity?> GetByIdTrackingIncludingAsync(int id, bool trackChanges = true, Expression<Func<TEntity, object?>>[]? includes = null, CancellationToken ct = default);
    //Task<TEntity?> GetByIdIncludingDeletedAsync(int id, CancellationToken ct = default);

    // Func vs Expression<Func>:
    //      - Func<T, bool> is executed in memory and cannot be translated to SQL.
    //      - Expression<Func<T, bool>> can be translated to SQL and executed on the database side.
    //Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);
    //Task<IReadOnlyList<TEntity>> FindTrackedAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);
    //Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);

    Task AddAsync(TEntity entity, CancellationToken ct = default);
    void Update(TEntity entity, CancellationToken ct = default);
    Task SoftDeleteAsync(TEntity entity, CancellationToken ct = default);

    Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken ct = default);

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

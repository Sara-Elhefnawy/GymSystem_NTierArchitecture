using GymSystem.Infrastructure.Entities;
using System.Linq.Expressions;

namespace GymSystem.Infrastructure.Repositories.Interfaces;

public interface IRepository<TEntity> where TEntity : BaseEntity
{
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken ct = default );

    Task<TEntity?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<TEntity?> GetByIdTrackingIncludingAsync(int id, bool trackChanges = true, Expression<Func<TEntity, object?>>[]? includes = null, CancellationToken ct = default);

    Task AddAsync(TEntity entity, CancellationToken ct = default);
    void Update(TEntity entity, CancellationToken ct = default);
    Task SoftDeleteAsync(TEntity entity, CancellationToken ct = default);

    Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken ct = default);

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

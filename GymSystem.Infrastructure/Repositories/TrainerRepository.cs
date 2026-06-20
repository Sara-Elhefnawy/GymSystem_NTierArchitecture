using GymSystem.Domain.Abstractions.Repositories;
using GymSystem.Infrastructure.Data;
using GymSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.Infrastructure.Repositories;

public class TrainerRepository(GymAppDbContext dbContext) : Repository<Trainer>(dbContext), ITrainerRepository
{
    private readonly DbSet<Trainer> _dbSet = dbContext.Set<Trainer>();

    public async Task<bool> IsEmailTakenAsync(string normalizedEmail, int? excludeMemberId = null, CancellationToken ct = default)
        => await _dbSet.AnyAsync(m => m.Email == normalizedEmail && (!excludeMemberId.HasValue || m.Id != excludeMemberId.Value), ct);

    public async Task<bool> IsPhoneTakenAsync(string phone, int? excludeMemberId = null, CancellationToken ct = default)
        => await _dbSet.AnyAsync(m => m.Phone == phone && (!excludeMemberId.HasValue || m.Id != excludeMemberId.Value), ct);

}

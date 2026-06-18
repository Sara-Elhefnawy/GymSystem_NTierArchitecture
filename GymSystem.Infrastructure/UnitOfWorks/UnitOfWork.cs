using GymSystem.Infrastructure.Data;
using GymSystem.Infrastructure.Repositories;
using GymSystem.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace GymSystem.Infrastructure.UnitOfWorks;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly GymAppDbContext _dbContext;
    private IDbContextTransaction? _transaction;

    private IMemberRepository? _members;
    private IPlanRepository? _plans;
    private ISessionRepository? _sessions;
    private IHealthRecordRepository? _healthRecords;
    private IBookingRepository? _bookings;
    private ITrainerRepository? _trainers;
    private ICategoryRepository? _categories;
    private IMembershipRepository? _memberships;


    public UnitOfWork(GymAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IMemberRepository Members
        => _members ??= new MemberRepository(_dbContext);

    public IPlanRepository Plans
        => _plans ??= new PlanRepository(_dbContext);

    public ITrainerRepository Trainers
        => _trainers ??= new TrainerRepository(_dbContext);

    public ISessionRepository Sessions
        => _sessions ??= new SessionRepository(_dbContext);

    public IBookingRepository Bookings
        => _bookings ??= new BookingRepository(_dbContext);

    public ICategoryRepository Categories
        => _categories ??= new CategoryRepository(_dbContext);

    public IHealthRecordRepository HealthRecords
        => _healthRecords ??= new HealthRecordRepository(_dbContext);

    public IMembershipRepository Memberships
        => _memberships ??= new MembershipRepository(_dbContext);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) 
        => await _dbContext.SaveChangesAsync(ct);

    public void Dispose() => _dbContext.Dispose();
}

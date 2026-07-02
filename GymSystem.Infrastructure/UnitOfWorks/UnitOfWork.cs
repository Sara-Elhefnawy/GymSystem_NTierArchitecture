using GymSystem.Domain.Abstractions.Repositories;
using GymSystem.Domain.Abstractions.UnitOfWorks;
using GymSystem.Infrastructure.Data;
using GymSystem.Infrastructure.Repositories;

namespace GymSystem.Infrastructure.UnitOfWorks;

public sealed class UnitOfWork(GymAppDbContext dbContext) : IUnitOfWork
{
    private IMemberRepository? _members;
    private IPlanRepository? _plans;
    private ISessionRepository? _sessions;
    private IHealthRecordRepository? _healthRecords;
    private IBookingRepository? _bookings;
    private ITrainerRepository? _trainers;
    private ICategoryRepository? _categories;
    private IMembershipRepository? _memberships;

    public IMemberRepository Members
        => _members ??= new MemberRepository(dbContext);

    public IPlanRepository Plans
        => _plans ??= new PlanRepository(dbContext);

    public ITrainerRepository Trainers
        => _trainers ??= new TrainerRepository(dbContext);

    public ISessionRepository Sessions
        => _sessions ??= new SessionRepository(dbContext);

    public IBookingRepository Bookings
        => _bookings ??= new BookingRepository(dbContext);

    public ICategoryRepository Categories
        => _categories ??= new CategoryRepository(dbContext);

    public IHealthRecordRepository HealthRecords
        => _healthRecords ??= new HealthRecordRepository(dbContext);

    public IMembershipRepository Memberships
        => _memberships ??= new MembershipRepository(dbContext);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await dbContext.SaveChangesAsync(ct);

    public void Dispose() => dbContext.Dispose();
}

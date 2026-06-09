using GymSystem.Infrastructure.Repositories;

namespace GymSystem.Infrastructure.UnitOfWorks;

public interface IUnitOfWork : IDisposable
{
    IMemberRepository Members { get; }

    IPlanRepository Plans { get; }

    ISessionRepository Sessions { get; }

    IHealthRecordRepository HealthRecords { get; }

    IBookingRepository Bookings { get; }

    ITrainerRepository Trainers { get; }

    ICategoryRepository Categories { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

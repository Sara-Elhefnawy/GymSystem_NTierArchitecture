using GymSystem.Domain.Abstractions.Repositories;

namespace GymSystem.Domain.Abstractions.UnitOfWorks;

public interface IUnitOfWork : IDisposable
{
    IMemberRepository Members { get; }

    IPlanRepository Plans { get; }

    ISessionRepository Sessions { get; }

    IHealthRecordRepository HealthRecords { get; }

    IBookingRepository Bookings { get; }

    ITrainerRepository Trainers { get; }

    ICategoryRepository Categories { get; }

    IMembershipRepository Memberships { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

using GymSystem.Infrastructure.Entities;

namespace GymSystem.Infrastructure.Repositories;

public interface IMemberRepository : IRepository<Member>
{
    Task<Member?> GetWithDetailsAsync(int id, CancellationToken ct = default);
    Task<Member?> GetWithHealthRecordAsync(int id, CancellationToken ct = default);
    Task<Member?> GetWithBookingsAsync(int id, CancellationToken ct = default);

    //Task<bool> HasEmailAsync(string email, CancellationToken ct = default);
    //Task<bool> HasPhoneAsync(string phone, CancellationToken ct = default);
}

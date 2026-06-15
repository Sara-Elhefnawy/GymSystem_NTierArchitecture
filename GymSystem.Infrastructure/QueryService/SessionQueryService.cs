using GymSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.Infrastructure.QueryService;

public class SessionQueryService(GymAppDbContext dbContext) : ISessionQueryService
{
    public async Task<IReadOnlyList<IndexSessionReadModel>> GetIndexItemsAsync(CancellationToken ct = default)
        => await dbContext.Sessions
            .AsNoTracking()
            .OrderBy(s => s.StartDate)
            .Select(s => new IndexSessionReadModel
            {
                Id = s.Id,
                CategoryName = s.Category.Name,
                Description = s.Description,
                TrainerName = s.Trainer.Name,
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                AvailableSlots = s.Bookings.Count,
                MaxCapacity = s.Capacity,
            })
            .ToListAsync(ct);
}

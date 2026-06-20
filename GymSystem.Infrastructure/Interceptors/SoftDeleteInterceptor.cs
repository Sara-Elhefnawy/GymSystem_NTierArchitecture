using GymSystem.Domain.Abstractions.Anonymization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace GymSystem.Infrastructure.Interceptor;

public class SoftDeleteInterceptor : SaveChangesInterceptor
{
    private readonly IAnonymizationService _anonymizationService;

    public SoftDeleteInterceptor(
        IAnonymizationService anonymizationService)
    {
        _anonymizationService = anonymizationService;
    }

    private void ApplySoftDelete(DbContext context)
    {
        var deletedEntries = context.ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Deleted)
            .ToList();

        foreach (var entry in deletedEntries)
        {
            var isDeletedProp = entry.Entity.GetType().GetProperty("IsDeleted");
            if (isDeletedProp is null) continue;

            entry.State = EntityState.Modified;
            isDeletedProp.SetValue(entry.Entity, true);

            var deletedAtProp = entry.Entity.GetType().GetProperty("DeletedAt");
            deletedAtProp?.SetValue(entry.Entity, DateTime.UtcNow);

            // ONLY anonymize if you need to free up unique constraints
            // Your filtered indexes already handle this!
            _anonymizationService.Anonymize(entry.Entity);
        }
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ApplySoftDelete(eventData.Context!);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplySoftDelete(eventData.Context!);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}

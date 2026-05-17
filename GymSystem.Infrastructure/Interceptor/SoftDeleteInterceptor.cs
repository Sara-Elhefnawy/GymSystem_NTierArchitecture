using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace GymSystem.Infrastructure.Interceptor;

public class SoftDeleteInterceptor : SaveChangesInterceptor
{
    private static void ApplySoftDelete(DbContext context)
    {
        var deletedEntries = context.ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Deleted)
            .ToList();

        foreach (var entry in deletedEntries)
        {
            // Soft delete: BaseEntity declares IsDeleted / DeletedAt (reflection still works)
            var isDeletedProp = entry.Entity.GetType().GetProperty("IsDeleted");
            var deletedAtProp = entry.Entity.GetType().GetProperty("DeletedAt");

            if (isDeletedProp is null) continue;   // no soft delete — allow hard delete

            // Convert Delete → Update with IsDeleted = true
            entry.State = EntityState.Modified;
            isDeletedProp.SetValue(entry.Entity, true);
            deletedAtProp?.SetValue(entry.Entity, DateTime.UtcNow);
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

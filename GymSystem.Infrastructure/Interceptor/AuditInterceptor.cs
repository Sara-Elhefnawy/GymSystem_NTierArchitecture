using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace GymSystem.Infrastructure.Interceptor;

public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly string _currentUser;

    public AuditInterceptor(string currentUser = "system")
    {
        _currentUser = currentUser;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context != null)
            SetAuditFields(eventData.Context);

        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context != null)
            SetAuditFields(eventData.Context);

        return ValueTask.FromResult(result);
    }

    private void SetAuditFields(DbContext context)
    {
        // Trainer: public DateTime HireDate { get; set; }    // employment start — business date; set in AuditInterceptor

        var now = DateTime.UtcNow;
        var currentUser = _currentUser;

        var entries = context.ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);
            //.Where(e => e.Entity is Course || e.Entity is Student);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property("CreatedAt").CurrentValue = now;
                entry.Property("UpdatedAt").CurrentValue = now;
                entry.Property("DeletedAt").CurrentValue = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property("UpdatedAt").CurrentValue = now;
                entry.Property("CreatedAt").IsModified = false;
            }
        }
    }
}

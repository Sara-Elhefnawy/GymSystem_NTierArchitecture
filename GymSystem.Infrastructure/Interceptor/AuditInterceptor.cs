using GymSystem.Infrastructure.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace GymSystem.Infrastructure.Interceptor;

public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditInterceptor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private void ApplyAudit(DbContext context)
    {
        var currentUser = _httpContextAccessor.HttpContext?.User?.Identity?.Name
                          ?? "System";
        var now = DateTime.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity is BaseEntity created)
                {
                    if (created.CreatedAt == default)
                        created.CreatedAt = now;
                }

                switch (entry.Entity)
                {
                    case Member m when m.JoinDate == default:
                        m.JoinDate = now.Date;   // "joined gym" — not the same concept as CreatedAt
                        break;
                    case Trainer t when t.HireDate == default:
                        t.HireDate = now.Date;
                        break;
                    case Membership ms when ms.StartDate == default:
                        ms.StartDate = now;
                        break;
                    case Booking bk when bk.BookingDate == default:
                        bk.BookingDate = now;
                        break;
                }

                //if (entry.Entity is GymUser user)
                //{
                //    user.CreatedBy = currentUser;
                //}
            }
            else if (entry.State == EntityState.Modified)
            {
                if (entry.Entity is BaseEntity modified)
                    modified.UpdatedAt = now;

                //if (entry.Entity is GymUser user)
                //    user.UpdatedBy = currentUser;
            }
        }
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ApplyAudit(eventData.Context!);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyAudit(eventData.Context!);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}

using Microsoft.EntityFrameworkCore;

namespace GymSystem.Domain.Abstractions.Interceptors;

public interface IAuditInterceptor
{
    void ApplyAudit(DbContext context);
}

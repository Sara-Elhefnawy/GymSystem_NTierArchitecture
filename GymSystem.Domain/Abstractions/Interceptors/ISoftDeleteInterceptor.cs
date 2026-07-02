using Microsoft.EntityFrameworkCore;

namespace GymSystem.Domain.Abstractions.Interceptors;

public interface ISoftDeleteInterceptor
{
    void ApplySoftDelete(DbContext context);
}

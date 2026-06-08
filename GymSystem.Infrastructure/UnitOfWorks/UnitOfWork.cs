using GymSystem.Infrastructure.Data;
using GymSystem.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace GymSystem.Infrastructure.UnitOfWorks;

public sealed class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly GymAppDbContext _dbContext;
    private readonly IServiceProvider _serviceProvider;

    private IMemberRepository? _members;
    private IPlanRepository? _plans;
    private ISessionRepository? _sessions;
    private IHealthRecordRepository? _healthRecords;

    public UnitOfWork(GymAppDbContext dbContext, IServiceProvider serviceProvider)
    {
        _dbContext = dbContext;
        _serviceProvider = serviceProvider;
    }

    public IMemberRepository Members 
        => _members ??= _serviceProvider.GetRequiredService<IMemberRepository>();

    public IPlanRepository Plans 
        => _plans ??= _serviceProvider.GetRequiredService<IPlanRepository>();

    public ISessionRepository Sessions 
        => _sessions ??= _serviceProvider.GetRequiredService<ISessionRepository>();

    public IHealthRecordRepository HealthRecords 
        => _healthRecords ??= _serviceProvider.GetRequiredService<IHealthRecordRepository>();

    public async Task<int> SaveChangesAsync() 
        => await _dbContext.SaveChangesAsync();

    public void Dispose() => _dbContext.Dispose();
}

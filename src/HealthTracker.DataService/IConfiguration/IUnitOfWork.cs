using HealthTracker.DataService.IRepository;

namespace HealthTracker.DataService.IConfiguration;

public interface IUnitOfWork
{
	IUsersRepository Users { get; }

	IRefreshTokensRepository RefreshTokens { get; }

	IHealthDataRepository HealthData { get; }

	Task CompleteAsync(); 
}

using HealthTracker.Entities.DbSet;

namespace HealthTracker.DataService.IRepository
{
	public interface IUsersRepository : IGenericRepository<User>
	{
		Task<bool> UpdateUserProfile(User user);
		Task<User> GetByIdentityId(Guid identityId);
	}
}

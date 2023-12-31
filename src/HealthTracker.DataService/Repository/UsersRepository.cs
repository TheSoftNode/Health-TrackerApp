
using HealthTracker.DataService.Data;
using HealthTracker.DataService.IRepository;
using HealthTracker.Entities.DbSet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HealthTracker.DataService.Repository
{
	public class UsersRepository : GenericRepository<User>, IUsersRepository
	{
		public UsersRepository(AppDbContext context, ILogger logger) : base(context, logger)
		{
		}

		public override async Task<IEnumerable<User>> All()
		{
			try
			{
				return await dbSet.Where(x => x.status == 1)
					.AsNoTracking()
					.ToListAsync();
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "{Repo} All method has generated an error", typeof(UsersRepository));
				return new List<User>();	
			}
		}

		public async Task<User> GetByIdentityId(Guid identityId)
		{
			try
			{
				return await dbSet.Where(
					x => x.status == 1
					&& x.IdentityId == identityId
					)
					.FirstOrDefaultAsync();

			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "{Repo} GetByIdentityId method has generated an error", typeof(UsersRepository));
				return null!;
			}
		}

		public async Task<bool> UpdateUserProfile(User user)
		{
			try
			{
				var existingUser =  await dbSet.Where(
					x => x.status == 1
					&& x.Id == user.Id
					).FirstOrDefaultAsync();

				if (existingUser == null) return false;

				existingUser.FirstName = user.FirstName;
				existingUser.LastName = user.LastName;
				existingUser.MobileNumber = user.MobileNumber;
				existingUser.Phone = user.Phone;
				existingUser.Sex = user.Sex;
				existingUser.Address = user.Address;
				existingUser.UpdateDate = DateTime.UtcNow;

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "{Repo}  UpdateUserProfile method has generated an error", typeof(UsersRepository));
				return false;
			}
		}
	}
}

using HealthTracker.DataService.Data;
using HealthTracker.DataService.IRepository;
using HealthTracker.Entities.DbSet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HealthTracker.DataService.Repository;

public class HealthDataRepository : GenericRepository<HealthData>, IHealthDataRepository
{
	public HealthDataRepository(
		AppDbContext context,
		ILogger logger ) : base( context, logger)
	{

	}


	public override async Task<IEnumerable<HealthData>> All()
	{
		try
		{
			return await dbSet.Where(x => x.status == 1)
				.AsNoTracking()
				.ToListAsync();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "{Repo} All method has generated an error", typeof(HealthDataRepository));
			return new List<HealthData>();
		}
	}

	public async Task<bool> UpdateHealthData(HealthData healthData)
	{
		try
		{
			var existingUser = await dbSet.Where(
				x => x.status == 1
				&& x.Id == healthData.Id
				).FirstOrDefaultAsync();

			if (existingUser == null) return false;

			existingUser.BloodType = healthData.BloodType;
			existingUser.Height = healthData.Height;
			existingUser.Race = healthData.Race;
			existingUser.weight = healthData.weight;
			existingUser.UseGlasses = healthData.UseGlasses;
			existingUser.UpdateDate = DateTime.UtcNow;

			return true;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "{Repo}  UpdateHealthData method has generated an error", typeof(UsersRepository));
			return false;
		}
	}
}


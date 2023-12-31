namespace HealthTracker.Entities.DbSet;

public abstract class BaseEntity
{
	public Guid Id { get; set; } = Guid.NewGuid();
	public int status { get; set; } = 1;
	public DateTime AddedDate { get; set; } = DateTime.UtcNow;
	public DateTime UpdateDate { get; set; }
}

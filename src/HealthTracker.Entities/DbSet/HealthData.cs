namespace HealthTracker.Entities.DbSet;

public class HealthData : BaseEntity
{
	public decimal Height { get; set; }
	public decimal weight { get; set; }
	public string BloodType { get; set; } // TODO: Make this information based on enum
	public string Race { get; set; }
	public bool UseGlasses { get; set; }
}

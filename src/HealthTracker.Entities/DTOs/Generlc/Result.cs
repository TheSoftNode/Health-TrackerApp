using HealthTracker.Entities.DTOs.Errors;

namespace HealthTracker.Entities.DTOs.Generlc;

public class Result<T>  // For single item return
{
	public T Content { get; set; }
	public Error Error { get; set; }
	public bool IsSuccess { get; set; } = true;
	public DateTime ResponseTime { get; set; } = DateTime.UtcNow;
}

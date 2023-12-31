namespace Health_Tracker.Authentication.Models.DTOs.Outgoing;

public class AuthResult
{
	public string Token { get; set; }
	public string RefreshToken { get; set; }
	public bool Result { get; set; }
	public bool Success { get; set; }
	public List<string> Errors { get; set; }
}

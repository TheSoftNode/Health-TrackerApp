using System.ComponentModel.DataAnnotations;

namespace Health_Tracker.Authentication.Models.DTOs.Incoming;

public class UserLoginRequestDto
{
	[Required]
	public string Email { get; set; }

	[Required]
	public string Password { get; set; }
}

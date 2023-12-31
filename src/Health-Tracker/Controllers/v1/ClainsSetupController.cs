using System.Security.Claims;
using AutoMapper;
using HealthTracker.DataService.IConfiguration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Health_Tracker.Controllers.v1;

public class ClainsSetupController : BaseController
{
	private readonly RoleManager<IdentityRole> _roleManager;
	private readonly ILogger<ClainsSetupController> _logger;
	public ClainsSetupController(
		IMapper mapper, 
		IUnitOfWork unitOfWork, 
		UserManager<IdentityUser> userManager,
		RoleManager<IdentityRole> roleManager,
		ILogger<ClainsSetupController> logger) 
		: base(mapper, unitOfWork, userManager)
	{
		_roleManager = roleManager;
		_logger = logger;
	}

	[HttpGet]
	public async Task<IActionResult> GetAllClaims(string email)
	{
		// Check if the user exist
		var user = await _userManager.FindByEmailAsync(email); 

		if (user == null)
		{
			_logger.LogInformation($"The user with the {email} does not exist");
			return BadRequest(new
			{
				error = "User does not exist"
			});
		}

		var userClaims = await _userManager.GetClaimsAsync (user);
		return Ok(userClaims);
	}

	[HttpPost]
	[Route("AddClaimsToUser")]
	public async Task<IActionResult> AddClaimsToUser(string email, string claimName, string claimValue)
	{
		// Check if the user exist
		var user = await _userManager.FindByEmailAsync (email);

		if (user == null)
		{
			_logger.LogInformation($"The user with the {email} does not exist");
			return BadRequest(new
			{
				error = "User does not exist"
			});
		}

		var userClaim = new Claim(claimName, claimValue);
		var result = await _userManager.AddClaimAsync(user, userClaim);

		if (result.Succeeded)
		{
			return Ok(new
			{
				result = $"User {user.Email} has a claim {claimName} added to them"
			});
		}

		return BadRequest( new
		{
			error = $"Unable to add claim {claimName} to the user {user.Email}"
		});
	}
}

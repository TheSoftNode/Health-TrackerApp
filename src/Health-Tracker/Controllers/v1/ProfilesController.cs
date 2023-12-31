using AutoMapper;
using Health_Tracker.Configuration.Messages;
using HealthTracker.DataService.IConfiguration;
using HealthTracker.Entities.DbSet;
using HealthTracker.Entities.DTOs.Errors;
using HealthTracker.Entities.DTOs.Generlc;
using HealthTracker.Entities.DTOs.Incoming.Profile;
using HealthTracker.Entities.DTOs.Outgoing.Profile;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Health_Tracker.Controllers.v1;


[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ProfilesController : BaseController
{
	public ProfilesController(
		IMapper mapper,
		IUnitOfWork unitOfWork,
		UserManager<IdentityUser> userManager
		) : base(mapper, unitOfWork, userManager)
	{
	}

	[HttpGet]
	public async Task<IActionResult> GetProfile()
	{
		var loggedInUser = await _userManager.GetUserAsync(HttpContext.User);
		var result = new Result<ProfileDto>();

		if (loggedInUser == null)
		{
			result.Error = PopulateError(400,
				ErrorMessages.Profile.UserNotFound,
				ErrorMessages.Generic.BadRequest);

			return BadRequest(result);
		}

		var identityId = new Guid(loggedInUser.Id);	

		var profile = await _unitOfWork.Users.GetByIdentityId(identityId);

		if (profile == null)
		{
			result.Error = PopulateError(400, 
				ErrorMessages.Profile.UserNotFound, 
				ErrorMessages.Generic.BadRequest);

			return BadRequest(result);
		}

		var mappedProfile  = _mapper.Map<ProfileDto>(profile);

		result.Content = mappedProfile;
		return Ok(result);
	}

	[HttpPut]
	public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto profile)
	{
		var result = new Result<ProfileDto>();

		// Check if the model is valid
		if (!ModelState.IsValid)
		{
			result.Error = PopulateError(400,
			ErrorMessages.Generic.InvalidPayload,
			ErrorMessages.Generic.BadRequest);

			return BadRequest(result);
		}

		var loggedInUser = await _userManager.GetUserAsync(HttpContext.User);

		if (loggedInUser == null)
		{
			result.Error = PopulateError(400,
			ErrorMessages.Profile.UserNotFound,
			ErrorMessages.Generic.BadRequest);

			return BadRequest(result);
		}

		var identityId = new Guid(loggedInUser.Id);

		var userProfile = await _unitOfWork.Users.GetByIdentityId(identityId);

		if (userProfile == null)
		{
			result.Error = PopulateError(400,
			ErrorMessages.Profile.UserNotFound,
			ErrorMessages.Generic.BadRequest);

			return BadRequest(result);
		}

		userProfile.Address = profile.Address;
		userProfile.Sex = profile.Sex;
		userProfile.MobileNumber = profile.MobileNumber;
		userProfile.Country = profile.Country;

		var isUpdated = await _unitOfWork.Users.UpdateUserProfile(userProfile);

		if (isUpdated)
		{
			await _unitOfWork.CompleteAsync();

			var mappedProfile = _mapper.Map<ProfileDto>(userProfile);

			result.Content = mappedProfile;
			return Ok(result);
		}

		result.Error = PopulateError( 500, 
			ErrorMessages.Generic.SomethingWentWrong, 
			ErrorMessages.Generic.UnableToProcess);

		return BadRequest(result);
	}
}

using AutoMapper;
using Health_Tracker.Configuration.Messages;
using Health_Tracker.Profiles;
using HealthTracker.DataService.IConfiguration;
using HealthTracker.Entities.DbSet;
using HealthTracker.Entities.DTOs.Generlc;
using HealthTracker.Entities.DTOs.Incoming;
using HealthTracker.Entities.DTOs.Outgoing.Profile;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Health_Tracker.Controllers.v1;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
public class UsersController : BaseController
{
    public UsersController(
        IMapper mapper,
        IUnitOfWork unitOfWork,
		UserManager<IdentityUser> userManager
		) : base(mapper, unitOfWork, userManager) // AppDbContext context)
    {}

    // Get All
    [HttpGet]
    [HttpHead]
    public async Task<IActionResult> GetUsers()
    {
        //var users = _context.Users.Where(x => x.status == 1).ToList();
        var users = await _unitOfWork.Users.All();

		var result = new PagedResult<User>
		{
			Content = users.ToList(),
			ResultCount = users.Count()
		};
		return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "DepartmentPolicy")]
    public async Task<IActionResult> AddUser(UserDto user)
    {
        var _mappedUser = _mapper.Map<User>(user);

        await _unitOfWork.Users.Add(_mappedUser);
        await _unitOfWork.CompleteAsync();

        //_context.Users.Add(_user);
        //_context.SaveChanges();

        var result = new Result<UserDto>();
        result.Content = user;  

        return CreatedAtRoute("GetUser", new { id = _mappedUser.Id }, result); // return 201
    }

    [HttpGet]
    [Route("GetUser", Name = "GetUser")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        //var user = _context.Users.FirstOrDefault(x => x.Id == id);
        var user = await _unitOfWork.Users.GetById(id);

        var result = new Result<ProfileDto>();
		if (user != null)
        {
			var mappedProfile = _mapper.Map<ProfileDto>(user);

			result.Content = mappedProfile;

			return Ok(result);
		}

        result.Error = PopulateError(404,
            ErrorMessages.Users.UserNotFound,
            ErrorMessages.Generic.DataNotFound);

        return BadRequest(result);
    }
}

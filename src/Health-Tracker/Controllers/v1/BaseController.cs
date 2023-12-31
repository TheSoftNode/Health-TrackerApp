using Asp.Versioning;
using AutoMapper;
using HealthTracker.DataService.IConfiguration;
using HealthTracker.Entities.DTOs.Errors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Health_Tracker.Controllers.v1
{
	[ApiVersion("1.0")]
	[Route("api/v{version:apiVersion}/[controller]")]
	[ApiController]
	public class BaseController : ControllerBase
	{
		//private readonly AppDbContext _context;
		protected IUnitOfWork _unitOfWork;
		protected readonly IMapper _mapper;
		public UserManager<IdentityUser> _userManager;

		public BaseController(
			IMapper mapper,
			IUnitOfWork unitOfWork, 
			UserManager<IdentityUser> userManager) // AppDbContext context)
		{
			//_context = context;
			_mapper = mapper; 
			_unitOfWork = unitOfWork;
			_userManager = userManager;
		}

		internal Error PopulateError(int code, string message, string type)
		{
			return new Error()
			{
				Code = code,
				Message = message,
				Type = type
			};
		}
	}
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Health_Tracker.Authentication.Configuration;
using Health_Tracker.Authentication.Models.DTOs.Generic;
using Health_Tracker.Authentication.Models.DTOs.Incoming;
using Health_Tracker.Authentication.Models.DTOs.Outgoing;
using HealthTracker.DataService.IConfiguration;
using HealthTracker.Entities.DbSet;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Health_Tracker.Controllers.v1;

public class AccountsController : BaseController
{
	private readonly TokenValidationParameters _tokenVlidationParameters;
	private readonly JwtConfig _jwtConfig;
	private readonly RoleManager<IdentityRole> _roleManager;
	private readonly ILogger<AccountsController> _logger;
	public AccountsController(
		IMapper mapper,
		IUnitOfWork unitOfWork,
		UserManager<IdentityUser> userManager,
		TokenValidationParameters tokenVlidationParameters,
		IOptionsMonitor<JwtConfig> optionsMonitor,
		RoleManager<IdentityRole> roleManager,
		ILogger<AccountsController> logger) : base(mapper, unitOfWork, userManager)
	{
		_jwtConfig = optionsMonitor.CurrentValue;
		_tokenVlidationParameters = tokenVlidationParameters;
		_roleManager = roleManager;
		_logger = logger;
	}

	[HttpPost]
	[Route("Register")]
	public async Task<IActionResult> Register([FromBody] UserRegistrationRequestDto registrationDto)
	{
		// Check that the model or object we are recieving is valid
		if (ModelState.IsValid)
		{
			// Check if the email already exists
			var userExist = await _userManager.FindByEmailAsync(registrationDto.Email);
			if (userExist != null) // Email is already in the table
			{
				return BadRequest(new UserRegistrationResponseDto
				{
					Success = false,
					Errors = new List<string>()
					{
						"Email already in use"
					}
				});
			}

			// Add the user
			var newUser = new IdentityUser()
			{
				Email = registrationDto.Email,
				UserName = registrationDto.Email,
				EmailConfirmed = true, // Todo: build email functionality to send to the user to confirm the email
			};

			// Adding the user to the table
			var isCreated = await _userManager.CreateAsync(newUser, registrationDto.Password);
			if (!isCreated.Succeeded) // When the registration has failed
			{
				return BadRequest(new UserRegistrationResponseDto
				{
					Success = isCreated.Succeeded,
					Errors = isCreated.Errors.Select(x => x.Description).ToList()
				});
			}

			// Adding user to the database
			var _user = new User
			{
				IdentityId = new Guid(newUser.Id),
				FirstName = registrationDto.FirstName,
				LastName = registrationDto.LastName,
				Email = registrationDto.Email,
				DateOfBirth = DateTime.UtcNow,
				Phone = "",
				Country = "",
				Address = "",
				MobileNumber = "",
				Sex = "",
				status = 1
			};

			// we need to add the user to a role
			await _userManager.AddToRoleAsync(newUser, "Admin");

			await _unitOfWork.Users.Add(_user);
			await _unitOfWork.CompleteAsync();

			// Create a Jwt token
			var token = await GenerateJwtToken(newUser);

			// Return the token to the user
			return Ok(new UserRegistrationResponseDto()
			{
				Success = true,
				Token = token.JwtToken,
				RefreshToken = token.RefreshToken
			});
		}
		else // Invalid Object
		{
			return BadRequest(new UserRegistrationResponseDto
			{
				Success = false,
				Errors = new List<string>()
				{
					"Invalid payload"
				}
			});
		}

	}

	[HttpPost]
	[Route("Login")]
	public async Task<IActionResult> Login([FromBody] UserLoginRequestDto loginDto)
	{
		if (ModelState.IsValid)
		{
			// 1 - Check if email exists
			var userExist = await _userManager.FindByEmailAsync(loginDto.Email);
			if (userExist == null) 
			{ 
				return BadRequest(new UserLoginResponseDto()
				{
					Success = false,
					Errors = new List<string>()
					{
						"Invalid authentication request"
					}
				});
			}

			// 2 - Check if the user has a valid password
			var isCorrect = await _userManager.CheckPasswordAsync(userExist, loginDto.Password);
			if (isCorrect)
			{
				// We need to generate a Jwt Token
				var jwtToken = await GenerateJwtToken(userExist);
				return Ok(new UserLoginResponseDto()
				{
					Success = true,
					Token = jwtToken.JwtToken,
					RefreshToken = jwtToken.RefreshToken
				}); 
			}
			else
			{
				// Password doesn't match
				return BadRequest(new UserLoginResponseDto()
				{
					Success = false,
					Errors = new List<string>()
					{
						"Invalid authentication request"
					}
				});
			}
		}
		else // Invalid Object
		{
			return BadRequest(new UserRegistrationResponseDto
			{
				Success = false,
				Errors = new List<string>()
				{
					"Invalid payload"
				}
			});
		}
	}

	[HttpPost]
	[Route("RefreshToken")]
	public async Task<IActionResult> RefreshToken([FromBody] TokenRequestDto tokenRequestDto)
	{
		if (ModelState.IsValid)
		{
			// Check if the refresh token is valid
			var result = await VerifyToken(tokenRequestDto);

			if (result == null)
			{
				return BadRequest(new UserRegistrationResponseDto()
				{
					Success = false,
					Errors = new List<string>()
					{
						"token validation failed"
					}
				});
			}
			return Ok(result);
		}
		else
		{
			return BadRequest(new UserRegistrationResponseDto()
			{
				Success = false,
				Errors = new List<string>()
				{
					"Invalid payload"
				}
			});
		}
	}

	private async Task<AuthResult> VerifyToken(TokenRequestDto tokenRequestDto)
	{
		var tokenHandler  = new JwtSecurityTokenHandler();

		try
		{
			// We need to check the validity of the token
			var principal = tokenHandler.ValidateToken(tokenRequestDto.Token, _tokenVlidationParameters, out var validatedToken);

			// We need to validate the results that has been generated for us
			// Validate if the string is an actual JWT token not a random string
			if (validatedToken is JwtSecurityToken jwtSecurityToken)
			{
				// Check if the jwt token is created with same algorithm as our jwt token
				var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);

				if (!result) return null!;
			}

			// We need to check the expiry date of the token
			var utcExpiryDate = long.Parse(principal.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);

			// Convert to date to check
			var expDate = UnixTimeStampToDateTime(utcExpiryDate);

			// Checking if the jwt token has expired
			if (expDate > DateTime.UtcNow)
			{
				return new AuthResult()
				{
					Success = false,
					Errors = new List<string>()
					{
						"Jwt token has not expired"
					}
				};
			}

			// Check if the refresh token exists
			var refreshTokenExist = await _unitOfWork.RefreshTokens.GetByRefreshToken(tokenRequestDto.RefreshToken);

			if (refreshTokenExist == null)
			{
				return new AuthResult()
				{
					Success = false,
					Errors = new List<string>()
					{
						"Invalid Refresh token"
					}
				};
			}

			// Check the expiry date of a refresh token
			if (refreshTokenExist.ExpiryDate < DateTime.UtcNow)
			{
				return new AuthResult()
				{
					Success = false,
					Errors = new List<string>()
					{
						" Refresh token has expired. Please login again"
					}
				};
			}

			// check if resfresh token has been used or not
			if (refreshTokenExist.IsUsed)
			{
				return new AuthResult()
				{
					Success = false,
					Errors = new List<string>()
					{
						" Refresh token has been used. It cannot be reused."
					}
				};
			}

			// Check if refresh token has been revoked
			if (refreshTokenExist.IsRevoked)
			{
				return new AuthResult()
				{
					Success = false,
					Errors = new List<string>()
					{
						" Refresh token has been revoked. It cannot be reused."
					}
				};
			}

			var jti = principal.Claims.SingleOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

			if (refreshTokenExist.JwtId != jti)
			{
				return new AuthResult()
				{
					Success = false,
					Errors = new List<string>()
					{
						" Refresh token reference does not match the jwt token.."
					}
				};
			}

			// Start processing and get a new token
			refreshTokenExist.IsUsed = true;

			var updateResult = await _unitOfWork.RefreshTokens.MarkRefreshTokenAsUsed(refreshTokenExist);
			
			if (updateResult)
			{
				await _unitOfWork.CompleteAsync();

				// Get the user to generate a new jwt token
				var dbUser = await _userManager.FindByIdAsync(refreshTokenExist.UserId);

				if (dbUser == null) 
				{
					return new AuthResult()
					{
						Success = false,
						Errors = new List<string>()
						{
							"Error Processing request"
						}
					};
				}

				// Generate a jwt token
				var tokens = await GenerateJwtToken(dbUser);

				return new AuthResult()
				{
					Token = tokens.JwtToken,
					Success = true,
					RefreshToken = tokens.RefreshToken
				};
			}

			return new AuthResult()
			{
				Success = false,
				Errors = new List<string>()
					{
						"Error Processing request"
					}
			};
		}
		catch(Exception ex)
		{
			// Todo: Add better error handling and add a logger
			return null!;
		}
	}

	private DateTime UnixTimeStampToDateTime(long unixDate)
	{
		// Set the time to 1, Jan, 1970
		var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
		
		// Add the number of seconds from dateTime
		dateTime = dateTime.AddSeconds(unixDate).ToUniversalTime();
		return dateTime;
	}

	private async Task<TokenData> GenerateJwtToken(IdentityUser user)
	{
		// Create the Jwt handler
		// The handler is going to be responsible for creating the token
		var jwtHandler = new JwtSecurityTokenHandler();

		// Get the security key
		var key = Encoding.UTF8.GetBytes(_jwtConfig.Secret);
		var claims = await GetAllValidClaims(user);

		var tokenDescriptor = new SecurityTokenDescriptor
		{

			Subject = new ClaimsIdentity(claims),
			//Subject = new ClaimsIdentity(new []
			//{

			//	//new Claim("Id", user.Id),
			//	//new Claim(ClaimTypes.NameIdentifier, user.Id),
			//	//new Claim(JwtRegisteredClaimNames.Sub, user.Email), // unique id
			//	//new Claim(JwtRegisteredClaimNames.Email, user.Email),
			//	//new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // used by the refresh token
			//}),
			Expires = DateTime.UtcNow.Add(_jwtConfig.ExpiryTimeFrame), //todo: update the expiration time to minutes
			SigningCredentials = new SigningCredentials
			(
				new SymmetricSecurityKey(key),
				SecurityAlgorithms.HmacSha256Signature // todo: review the algorithm down the road
			)
		};

		// Generate the security token 
		var token = jwtHandler.CreateToken(tokenDescriptor);

		// Convert the security obj token into a string
		var jwtToken = jwtHandler.WriteToken(token);

		// Generate a refresh token
		var refreshToken = new RefreshToken
		{
			AddedDate = DateTime.UtcNow,
			Token = $"{RandomStringGenerator(25)}_{Guid.NewGuid()}", // Create a method to generate a random string and attach a certain guid
			UserId = user.Id,
			IsRevoked = false,
			IsUsed = false,
			status = 1,
			JwtId = token.Id,
			ExpiryDate = DateTime.UtcNow.AddMonths(6)
		};

		await _unitOfWork.RefreshTokens.Add(refreshToken);
		await _unitOfWork.CompleteAsync();

		var tokenData = new TokenData
		{
			JwtToken = jwtToken,
			RefreshToken = refreshToken.Token
		};

		return tokenData;
	}

	private async Task<List<Claim>> GetAllValidClaims(IdentityUser user)
	{
		var _options = new IdentityOptions();
		var claims = new List<Claim>
		{
			new Claim("Id", user.Id),
			new Claim(ClaimTypes.NameIdentifier, user.Id),
			new Claim(JwtRegisteredClaimNames.Sub, user.Email), // unique id
			new Claim(JwtRegisteredClaimNames.Email, user.Email),
			new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // used by the refresh token
		};

		// Getting the claims that we have assigned to the user
		var userClaims = await _userManager.GetClaimsAsync(user);
		claims.AddRange(userClaims);

		// Get the user role and add it to the claims
		var userRoles = await _userManager.GetRolesAsync(user);

		foreach(var userRole in userRoles)
		{
			var role = await _roleManager.FindByNameAsync(userRole);

			if (role != null)
			{
				claims.Add(new Claim(ClaimTypes.Role, userRole));
				var roleClaims = await _roleManager.GetClaimsAsync(role);
				foreach(var roleClaim in roleClaims)
				{
					claims.Add(roleClaim);
				}
			}
		}
		return claims;
	}

	private string RandomStringGenerator(int length)
	{
		 var random = new Random();
		const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

		return new string(Enumerable.Repeat(chars, length)
			.Select(s => s[random.Next(s.Length)]).ToArray());
	}

}

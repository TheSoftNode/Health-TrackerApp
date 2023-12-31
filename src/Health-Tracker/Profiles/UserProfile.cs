using AutoMapper;
using HealthTracker.Entities.DbSet;
using HealthTracker.Entities.DTOs.Incoming;
using HealthTracker.Entities.DTOs.Outgoing.Profile;

namespace Health_Tracker.Profiles;

public class UserProfile : Profile
{
    public UserProfile()
    {
		CreateMap<UserDto, User>()
			.ForMember(
				dest => dest.FirstName,
				from => from.MapFrom(x => $"{x.FirstName}")
			)
			.ForMember(
				dest => dest.LastName,
				from => from.MapFrom(x => $"{x.LastName}")
			)
			.ForMember(
				dest => dest.Email,
				from => from.MapFrom(x => $"{x.Email}")
			)
			.ForMember(
				dest => dest.Phone,
				from => from.MapFrom(x => $"{x.Phone}")
			)
			.ForMember(
				dest => dest.DateOfBirth,
				from => from.MapFrom(x => Convert.ToDateTime(x.DateOfBirth))
			)
			.ForMember(
				dest => dest.Country,
				from => from.MapFrom(x => $"{x.Country}")
			)
			.ForMember(
				dest => dest.status,
				from => from.MapFrom(x => 1)
			);

		CreateMap<User, ProfileDto>()
			.ForMember(
				dest => dest.FirstName,
				from => from.MapFrom(x => $"{x.FirstName}")
			)
			.ForMember(
				dest => dest.LastName,
				from => from.MapFrom(x => $"{x.LastName}")
			)
			.ForMember(
				dest => dest.Email,
				from => from.MapFrom(x => $"{x.Email}")
			)
			.ForMember(
				dest => dest.Phone,
				from => from.MapFrom(x => $"{x.Phone}")
			)
			.ForMember(
				dest => dest.DateOfBirth,
				from => from.MapFrom(x => $"{x.DateOfBirth.ToShortDateString()}")
			)
			.ForMember(
				dest => dest.Country,
				from => from.MapFrom(x => $"{x.Country}")
			);
	}
}

using AutoMapper;
using Unstapp.Application.DTOs;
using Unstapp.Infrastructure.Entities;

namespace Unstapp.Application.Mappings
{
    public class AuthProfile : Profile
    {
        public AuthProfile()
        {
            CreateMap<RegisterRequestDto, User>()
                .ForMember(dest => dest.Password, opt => opt.Ignore())
                .ForMember(dest => dest.FirstTime, opt => opt.MapFrom(_ => true));

            CreateMap<User, LoginResponseDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.Name} {src.LastName}"))
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.UserRoles.Select(ur => ur.Role.Name).ToList()));
        }
    }
}

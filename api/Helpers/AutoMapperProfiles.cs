using api.DTOs;
using api.Entities;
using api.Extensions;
using AutoMapper;

namespace api.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<AppUser, MemberDto>()
                .ForMember(
                    dest => dest.PhotoUrl, 
                    opt => opt.MapFrom(src => src.Photos.FirstOrDefault(x => x.IsMain).Url)
                )
                .ForMember(
                    dest => dest.Age, 
                    opt => opt.MapFrom(src => src.DateOfBirth.CalculateAge())
                );

            CreateMap<Photo, PhotoDto>();

            // because the property names of MemberUpdateDto match exactly with what we have inside
            // the AppUser, we do not need to add any additional configuration
            CreateMap<MemberUpdateDto, AppUser>();
        }
    }
}
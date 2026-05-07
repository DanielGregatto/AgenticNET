using AutoMapper;
using Services.Contracts.Results;
using Identity.Model;
using Services.Features.Account.Commands.UpdateAddress;
using Services.Features.Account.Commands.UpdatePersonalInfo;

namespace UI.API.Configurations
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ApplicationUser, ProfileResult>().ReverseMap();

            CreateMap<ApplicationUser, UpdatePersonalInfoCommand>().ReverseMap();

            CreateMap<ApplicationUser, UpdateAddressCommand>().ReverseMap();
        }
    }
}
using AutoMapper;
using EstateHub.Authorization.Domain.DTO.Authentication.Responses;

namespace EstateHub.Authorization.Domain;

public class DomainMappingProfile : Profile
{
    public DomainMappingProfile()
    {
        CreateMap<AuthenticationResponse, AuthenticationResult>().ReverseMap();
    }
}
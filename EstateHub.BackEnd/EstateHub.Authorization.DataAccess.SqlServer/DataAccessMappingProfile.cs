using AutoMapper;
using EstateHub.Authorization.Domain.DTO.Authentication.Responses;
using EstateHub.Authorization.Domain.DTO.User;
using EstateHub.Authorization.Domain.DTO.Session;
using EstateHub.Authorization.Domain.Models;
using EstateHub.Authorization.DataAccess.SqlServer.Entities;
using EstateHub.SharedKernel.Contracts.AuthorizationMicroservice.Responses;

namespace EstateHub.Authorization.DataAccess.SqlServer;

public class DataAccessMappingProfile : Profile
{
    public DataAccessMappingProfile()
    {
        CreateMap<UserEntity, UserEntity>();

        CreateMap<AuthenticationResult, AuthenticationResponse>();

        CreateMap<UserEntity, GetUserResponse>()
            .ForMember(dst => dst.Avatar,
                opt => opt.MapFrom(src => User.ConvertAvatarToDataUri(src.AvatarData, src.AvatarContentType)));

        CreateMap<UserEntity, GetUserWithRolesResponse>()
            .ForMember(dst => dst.Avatar,
                opt => opt.MapFrom(src => User.ConvertAvatarToDataUri(src.AvatarData, src.AvatarContentType)))
            .ForMember(dst => dst.Roles,
                opt => opt.MapFrom(src => src.UserRoles.Select(ur => ur.Role.Name)));

        CreateMap<UserEntity, UserDto>();
        CreateMap<UserEntity, UserWithRolesDto>()
            .ForMember(dst => dst.Roles,
                opt => opt.MapFrom(src => src.UserRoles.Select(ur => ur.Role.Name)));

        CreateMap<SessionEntity, Session>().ReverseMap();
        CreateMap<SessionEntity, SessionEntity>();
        CreateMap<SessionEntity, SessionDto>();
    }
}

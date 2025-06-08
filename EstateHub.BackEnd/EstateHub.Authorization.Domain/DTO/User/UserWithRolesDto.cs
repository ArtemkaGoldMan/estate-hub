namespace EstateHub.Authorization.Domain.DTO.User;

public class UserWithRolesDto : UserDto
{
    public List<string> Roles { get; set; }
}